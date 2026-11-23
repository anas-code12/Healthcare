using Healthcare.Dtos.Appointments;
using Healthcare.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HealthcareSystemContext _context;

    public AppointmentService(HealthcareSystemContext context)
    {
        _context = context;
    }

    // ================================================================
    // Helper: تحويل الـ JSON لـ MedicineDetailDto
    // ================================================================
    private List<MedicineDetailDto> ParseMedicines(string? medicinesJson, List<Medicine> allMedicines)
    {
        if (string.IsNullOrEmpty(medicinesJson)) return new();
        try
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<MedicineRequest>>(medicinesJson);
            if (items == null) return new();
            return items.Select(m => new MedicineDetailDto
            {
                MedicineId = m.MedicineId,
                MedicineName = allMedicines.FirstOrDefault(x => x.MedicineId == m.MedicineId)?.MedicineName,
                Dose = m.Dose,
                Form = m.Form,
                Frequency = m.Frequency,
                Days = m.Days,
                Timing = m.Timing
            }).ToList();
        }
        catch { return new(); }
    }

    // ================================================================
    // جيب المواعيد المتاحة للدكتور لعيادة معينة لمدة 14 يوم
    // ================================================================
    public async Task<DoctorAvailabilityResponse> GetDoctorAvailabilityAsync(int doctorId, int clinicId)
    {
        await AutoCancelExpiredAppointmentsAsync();
        var doctor = await _context.Doctors
            .Include(d => d.Clinics)
            .ThenInclude(c => c.Breaks)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null)
            throw new Exception("الدكتور مش موجود");

        var clinic = doctor.Clinics.FirstOrDefault(c => c.ClinicId == clinicId);

        if (clinic == null)
            throw new Exception("العيادة مش موجودة أو مش بتاعت الدكتور ده");

        if (clinic.WorkStartTime == null || clinic.WorkEndTime == null
            || string.IsNullOrEmpty(clinic.WorkingDays))
            throw new Exception("العيادة مش مكتملة البيانات");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var endDate = today.AddDays(14);

        var bookedAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.ClinicId == clinicId
                     && a.AppointmentDate >= today
                     && a.AppointmentDate <= endDate
                     && a.Status != "Cancelled")
            .Select(a => new { a.AppointmentDate, a.AppointmentTime })
            .ToListAsync();

        var workingDaysList = clinic.WorkingDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim())
            .ToList();

        var breakTimes = clinic.Breaks.Select(b => b.BreakTime).ToList();
        var availableSlots = new List<DaySlots>();

        for (int i = 0; i < 14; i++)
        {
            var date = today.AddDays(i);
            var dayName = date.DayOfWeek.ToString();

            if (!workingDaysList.Contains(dayName))
                continue;

            var bookedForDay = bookedAppointments
                .Where(a => a.AppointmentDate == date)
                .Select(a => a.AppointmentTime)
                .ToList();

            var slots = GenerateSlots(
                clinic.WorkStartTime.Value,
                clinic.WorkEndTime.Value,
                clinic.SlotDurationMinutes,
                bookedForDay,
                breakTimes,
                date
            );

            availableSlots.Add(new DaySlots
            {
                Date = date.ToString("yyyy-MM-dd"),
                Slots = slots
            });
        }

        return new DoctorAvailabilityResponse
        {
            DoctorId = doctor.DoctorId,
            DoctorName = doctor.DoctorName,
            ClinicId = clinic.ClinicId,
            ClinicName = clinic.ClinicName,
            ClinicPhone = clinic.ClinicPhone,
            ClinicAddress = clinic.Location,
            DoctorPrice = clinic.DoctorPrice,
            WorkingDays = workingDaysList,
            AvailableSlots = availableSlots
        };
    }

    // ================================================================
    // Helper: ولّد الـ slots مع البريك
    // ================================================================
    private static List<SlotDto> GenerateSlots(
        TimeSpan startTime,
        TimeSpan endTime,
        int slotMinutes,
        List<TimeOnly?> bookedTimes,
        List<TimeOnly> breakTimes,
        DateOnly date)
    {
        var slots = new List<SlotDto>();
        var current = TimeOnly.FromTimeSpan(startTime);
        var end = TimeOnly.FromTimeSpan(endTime);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var today = DateOnly.FromDateTime(DateTime.Today);

        while (current.AddMinutes(slotMinutes) <= end)
        {
            var isBooked = bookedTimes.Any(b => b.HasValue && b.Value == current);
            var isBreak = breakTimes.Contains(current);
            var isPast = date == today && current <= now;

            slots.Add(new SlotDto
            {
                Time = current.ToString("hh:mm tt"),
                Available = !isBooked && !isBreak && !isPast
            });

            current = current.AddMinutes(slotMinutes);
        }

        return slots;
    }

    // ================================================================
    // احجز موعد
    // ================================================================
    public async Task<AppointmentResponse> BookAppointmentAsync(int patientId, BookAppointmentRequest request)
    {

        await AutoCancelExpiredAppointmentsAsync();
        // ✅ تحقق من البلوك
        var oneWeekAgo = DateTime.Now.AddDays(-7);
        var cancelledCount = await _context.Appointments
            .CountAsync(a => a.PatientId == patientId
                          && a.Status == "Cancelled"
                          && a.UpdatedAt >= oneWeekAgo);

        if (cancelledCount >= 3)
            throw new Exception("تم إيقاف حسابك مؤقتاً لمدة أسبوع بسبب إلغاء 3 مواعيد أو أكثر");

        // ✅ تحقق إن الحجز قبل الموعد بساعة على الأقل
        var appointmentDateTime = request.AppointmentDate.ToDateTime(request.AppointmentTime);
        if (appointmentDateTime <= DateTime.Now.AddHours(1))
            throw new Exception("لازم تحجز قبل الموعد بساعة على الأقل");

        var doctor = await _context.Doctors
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);

        if (doctor == null)
            throw new Exception("الدكتور مش موجود");

        var clinic = await _context.Clinics
            .Include(c => c.Breaks)
            .FirstOrDefaultAsync(c => c.ClinicId == request.ClinicId && c.DoctorId == request.DoctorId);

        if (clinic == null)
            throw new Exception("العيادة مش موجودة");

        var isBreak = clinic.Breaks.Any(b => b.BreakTime == request.AppointmentTime);
        if (isBreak)
            throw new Exception("الوقت ده وقت راحة للدكتور، اختار موعد تاني");

        var isBooked = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.ClinicId == request.ClinicId &&
            a.AppointmentDate == request.AppointmentDate &&
            a.AppointmentTime == request.AppointmentTime &&
            a.Status != "Cancelled");

        if (isBooked)
            throw new Exception("الموعد ده محجوز بالفعل، اختار موعد تاني");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            ClinicId = request.ClinicId,
            AppointmentDate = request.AppointmentDate,
            AppointmentTime = request.AppointmentTime,
            DurationMinutes = clinic.SlotDurationMinutes,
            Status = "Pending",
            CreatedAt = DateTime.Now
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return new AppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            DoctorId = doctor.DoctorId,
            ClinicId = clinic.ClinicId,
            DoctorName = doctor.DoctorName,
            Specialty = doctor.Specialty?.SpecialtyName,
            Rating = doctor.AverageRating,
            Location = clinic.Location,
            Price = clinic.DoctorPrice,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime?.ToString("hh:mm tt"),
            Status = appointment.Status,
        };
    }

    // ================================================================
    // مواعيد المريض
    // ================================================================
    public async Task<List<AppointmentResponse>> GetPatientAppointmentsAsync(int patientId)
    {
        var appointments = await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialty)
            .Include(a => a.Clinic)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        var allDiseases = await _context.Diseases.ToListAsync();
        var allMedicines = await _context.Medicines.ToListAsync();

        return appointments.Select(a =>
        {
            List<string>? diseaseNames = null;
            List<MedicineDetailDto>? medicines = null;

            if (a.Status == "Completed")
            {
                if (!string.IsNullOrEmpty(a.DiseasesId))
                {
                    var ids = a.DiseasesId.Split(',')
                        .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                        .Where(id => id > 0).ToList();
                    diseaseNames = allDiseases
                        .Where(d => ids.Contains(d.DiseaseId))
                        .Select(d => d.DiseasesName ?? "")
                        .ToList();
                }
                medicines = ParseMedicines(a.MedicinesJson, allMedicines);
            }

            return new AppointmentResponse
            {
                AppointmentId = a.AppointmentId,
                DoctorId = a.DoctorId,
                ClinicId = a.ClinicId,
                DoctorName = a.Doctor?.DoctorName,
                Specialty = a.Doctor?.Specialty?.SpecialtyName,
                Rating = a.Doctor?.AverageRating,
                Location = a.Clinic?.Location,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime?.ToString("hh:mm tt"),
                Status = a.Status,
                PatientRating = a.PatientRating,
                DiseaseNames = diseaseNames,
                Medicines = medicines,
                DiseaseDescription = a.Status == "Completed" ? a.DiseaseDescription : null,
                Notes = a.Status == "Completed" ? a.Notes : null,
            };
        }).ToList();
    }

    // ================================================================
    // مواعيد الدكتور
    // ================================================================
    public async Task<List<DoctorAppointmentResponse>> GetDoctorAppointmentsAsync(int doctorId)
    {
        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Include(a => a.Patient)
                .ThenInclude(p => p.PatientSickRecord)
            .Include(a => a.Clinic)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(a => new DoctorAppointmentResponse
        {
            AppointmentId = a.AppointmentId,
            PatientId = a.PatientId,
            PatientName = a.Patient?.PatientName,
            ClinicName = a.Clinic?.ClinicName,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime?.ToString("hh:mm tt"),
            Status = a.Status,
            PatientAge = a.Patient?.PatientAge,
            PatientGender = a.Patient?.PatientGender == "M" ? "Male" :
                a.Patient?.PatientGender == "F" ? "Female" : null,
            PatientCity = a.Patient?.PatientCity,
            PatientMarried = a.Patient?.PatientMarried,
            PatientPhone = a.Patient?.PatientPhone,
            PatientHeight = a.Patient?.PatientSickRecord?.PationtHight,
            PatientWeight = a.Patient?.PatientSickRecord?.PationtWeight,
            PatientAnaemia = a.Patient?.PatientSickRecord?.PatientAnaemia,
            PatientHypertension = a.Patient?.PatientSickRecord?.PatientHypertension,
            PatientDiabetes = a.Patient?.PatientSickRecord?.PatientDiabetes,
            PatientBloodtype = a.Patient?.PatientSickRecord?.PatientBloodtype
        }).ToList();
    }

    // ================================================================
    // إلغاء موعد
    // ================================================================
    public async Task<bool> CancelAppointmentAsync(int appointmentId, int patientId)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId
                                   && a.PatientId == patientId);

        if (appointment == null)
            throw new Exception("الموعد مش موجود أو مش بتاعك");

        appointment.Status = "Cancelled";
        appointment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // جيب الأمراض والأدوية بتاعت التخصص
    // ================================================================
    public async Task<object> GetDiseasesAndMedicinesByDoctorAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null || doctor.SpecialtyId == null)
            throw new Exception("الدكتور مش موجود أو مفيش تخصص");

        var diseases = await _context.Diseases
            .Where(d => d.SpecialtyId == doctor.SpecialtyId)
            .Select(d => new DiseaseDto
            {
                DiseaseId = d.DiseaseId,
                DiseasesName = d.DiseasesName,
                diseaseSeverity = d.disease_severity
            })
            .ToListAsync();

        var medicines = await _context.Medicines
            .Where(m => m.SpecialtyId == doctor.SpecialtyId)
            .Select(m => new MedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName
            })
            .ToListAsync();

        return new { Diseases = diseases, Medicines = medicines };
    }

    // ================================================================
    // إنهاء الكشف
    // ================================================================
    public async Task<bool> CompleteAppointmentAsync(int appointmentId, int doctorId, CompleteAppointmentRequest req)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId
                                   && a.DoctorId == doctorId
                                   && a.Status == "Pending");

        if (appointment == null)
            throw new Exception("الموعد مش موجود أو مش بتاعك أو مش Pending");

        appointment.Notes = req.Notes;
        appointment.DiseaseDescription = req.DiseaseDescription;
        appointment.DiseasesId = req.DiseasesIds != null
            ? string.Join(",", req.DiseasesIds) : null;
        appointment.MedicinesJson = req.Medicines != null
            ? System.Text.Json.JsonSerializer.Serialize(req.Medicines) : null;
        appointment.Status = "Completed";
        appointment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // pending-diagnosis - للـ AI team
    // ================================================================
    public async Task<List<object>> GetPendingDiagnosisAsync()
    {
        return await _context.Appointments
            .Where(a => a.Status == "Completed"
                     && a.DiseaseDescription != null
                     && a.AiDiagnosis == null)
            .Select(a => new
            {
                appointmentId = a.AppointmentId,
                diseaseDescription = a.DiseaseDescription
            })
            .Cast<object>()
            .ToListAsync();
    }

    // ================================================================
    // chronic-disease-stats - للـ AI team
    // ================================================================
    public async Task<List<object>> GetChronicDiseaseStatsAsync()
    {
        var chronicDiseases = new List<string>
        {
            "Chronic Kidney Disease", "COVID-19", "Rheumatoid Arthritis",
            "Coronary Artery Disease", "Hypertension", "Diabetes Mellitus",
            "Alzheimer's Disease", "Glaucoma", "Liver Cirrhosis",
            "Heart Failure", "COPD", "Psoriasis", "Epilepsy", "PCOS",
            "HIV/AIDS", "Stroke", "Prostate Cancer", "Breast Cancer",
            "Parkinson's Disease"
        };

        var diseases = await _context.Diseases
            .Where(d => chronicDiseases.Contains(d.DiseasesName!))
            .ToListAsync();

        var appointments = await _context.Appointments
            .Where(a => a.Status == "Completed"
                     && a.DiseasesId != null
                     && a.AiDiagnosis == null)
            .Include(a => a.Patient)
            .ToListAsync();

        var result = new List<object>();

        foreach (var appointment in appointments)
        {
            if (string.IsNullOrEmpty(appointment.DiseasesId)) continue;

            var diseaseIds = appointment.DiseasesId.Split(',')
                .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                .Where(id => id > 0).ToList();

            var matchedDiseases = diseases
                .Where(d => diseaseIds.Contains(d.DiseaseId))
                .ToList();

            foreach (var disease in matchedDiseases)
            {
                result.Add(new
                {
                    Patient_ID = appointment.PatientId,
                    Disease = disease.DiseasesName,
                    Gender = appointment.Patient?.PatientGender == "M" ? "Male" :
                             appointment.Patient?.PatientGender == "F" ? "Female" : null,
                    Governorate = appointment.Patient?.PatientCity,
                    Age = appointment.Patient?.PatientAge,
                    Year = appointment.AppointmentDate?.Year.ToString(),
                    Month = appointment.AppointmentDate?.Month.ToString()
                });
            }
        }

        return result;
    }

    // ================================================================
    // AI Team بيخزن الرد
    // ================================================================
    public async Task<bool> SaveAiDiagnosisAsync(AiDiagnosisRequest req)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == req.AppointmentId);

        if (appointment == null)
            throw new Exception("الموعد مش موجود");

        appointment.AiDiagnosis = req.Diagnosis;
        await _context.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // وزارة الصحة
    // ================================================================
    public async Task<List<HealthMinistryResponse>> GetHealthMinistryDataAsync()
    {
        return await _context.Appointments
            .Where(a => a.Status == "Completed" && a.AiDiagnosis != null)
            .Include(a => a.Patient)
            .Select(a => new HealthMinistryResponse
            {
                AppointmentId = a.AppointmentId,
                DiseaseDescription = a.DiseaseDescription,
                AiDiagnosis = a.AiDiagnosis,
                AppointmentDate = a.AppointmentDate,
                PatientName = a.Patient!.PatientName,
                PatientAge = a.Patient.PatientAge,
                PatientGender = a.Patient.PatientGender == "M" ? "Male" :
                               a.Patient.PatientGender == "F" ? "Female" : null,
                PatientCity = a.Patient.PatientCity,
                PatientPhone = a.Patient.PatientPhone,
                PatientMarried = a.Patient.PatientMarried
            })
            .ToListAsync();
    }

    // ================================================================
    // تقييم الدكتور
    // ================================================================
    public async Task<bool> RateAppointmentAsync(int appointmentId, int patientId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new Exception("التقييم لازم يكون بين 1 و 5");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId
                                   && a.PatientId == patientId
                                   && a.Status == "Completed");

        if (appointment == null)
            throw new Exception("الموعد مش موجود أو مش مكتمل");

        if (appointment.PatientRating != null)
            throw new Exception("انت عملت تقييم قبل كده");

        appointment.PatientRating = rating;
        appointment.UpdatedAt = DateTime.Now;

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == appointment.DoctorId);

        if (doctor != null)
        {
            var totalRatings = (doctor.TotalRatings ?? 0) + 1;
            var avgRating = ((doctor.AverageRating ?? 0) * (totalRatings - 1) + rating) / totalRatings;
            doctor.TotalRatings = totalRatings;
            doctor.AverageRating = Math.Round(avgRating, 2);
        }

        await _context.SaveChangesAsync();
        return true;
    }
    // ================================================================
    // إلغاء تلقائي للمواعيد اللي فاتت ومحدش كشف عليها
    // ================================================================
    public async Task<int> AutoCancelExpiredAppointmentsAsync()
    {
        var now = DateTime.Now;

        var expiredAppointments = await _context.Appointments
            .Where(a => a.Status == "Pending")
            .ToListAsync();

        var toCancel = expiredAppointments
            .Where(a => a.AppointmentDate.HasValue && a.AppointmentTime.HasValue
                     && a.AppointmentDate.Value.ToDateTime(a.AppointmentTime.Value) < now)
            .ToList();

        foreach (var appt in toCancel)
        {
            appt.Status = "Cancelled";
            appt.UpdatedAt = now;
        }

        if (toCancel.Count > 0)
            await _context.SaveChangesAsync();

        return toCancel.Count;
    }

    // ================================================================
    // السجل الطبي للمريض
    // ================================================================
    public async Task<List<PatientMedicalHistoryResponse>> GetPatientMedicalHistoryAsync(int patientId)
    {
        var appointments = await _context.Appointments
            .Where(a => a.PatientId == patientId && a.Status == "Completed")
            .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialty)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        var allDiseases = await _context.Diseases.ToListAsync();
        var allMedicines = await _context.Medicines.ToListAsync();

        return appointments.Select(a =>
        {
            List<string>? diseaseNames = null;
            if (!string.IsNullOrEmpty(a.DiseasesId))
            {
                var ids = a.DiseasesId.Split(',')
                    .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0).ToList();
                diseaseNames = allDiseases
                    .Where(d => ids.Contains(d.DiseaseId))
                    .Select(d => d.DiseasesName ?? "")
                    .ToList();
            }

            return new PatientMedicalHistoryResponse
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor?.DoctorName,
                Specialty = a.Doctor?.Specialty?.SpecialtyName,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime?.ToString("hh:mm tt"),
                DiseaseNames = diseaseNames,
                Medicines = ParseMedicines(a.MedicinesJson, allMedicines),
                DiseaseDescription = a.DiseaseDescription,
                Notes = a.Notes
            };
        }).ToList();
    }
}