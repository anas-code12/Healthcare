using Healthcare.Dtos.Clinics;
using Healthcare.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Services;

public class ClinicService
{
    private readonly HealthcareSystemContext _context;

    public ClinicService(HealthcareSystemContext context)
    {
        _context = context;
    }

    // ================================================================
    // جيب عيادات الدكتور
    // ================================================================
    public async Task<List<ClinicResponse>> GetDoctorClinicsAsync(int doctorId)
    {
        var clinics = await _context.Clinics
            .Where(c => c.DoctorId == doctorId)
            .Include(c => c.Breaks)
            .Include(c => c.Doctor) // ✅ زودنا Include للدكتور
            .ToListAsync();

        return clinics.Select(MapToResponse).ToList();
    }

    // ================================================================
    // إضافة عيادة جديدة
    // ================================================================
    public async Task<ClinicResponse> AddClinicAsync(int doctorId, ClinicUpsertRequest request)
    {
        if (request.WorkingDays != null && request.WorkStartTime != null && request.WorkEndTime != null)
        {
            await ValidateNoOverlapAsync(doctorId, null, request);
        }

        var clinic = new Clinic
        {
            DoctorId = doctorId,
            ClinicName = request.ClinicName,
            ClinicPhone = request.ClinicPhone,
            ClinicEmail = request.ClinicEmail,
            Location = request.Location,
            WorkingDays = request.WorkingDays,
            WorkStartTime = ParseTime(request.WorkStartTime),
            WorkEndTime = ParseTime(request.WorkEndTime),
            DoctorPrice = request.DoctorPrice,
            SlotDurationMinutes = request.SlotDurationMinutes ?? 30
        };

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        return MapToResponse(clinic);
    }

    // ================================================================
    // تعديل عيادة
    // ================================================================
    public async Task<ClinicResponse> UpdateClinicAsync(int doctorId, int clinicId, ClinicUpsertRequest request)
    {
        var clinic = await _context.Clinics
            .Include(c => c.Breaks)
            .Include(c => c.Doctor) // ✅ زودنا Include للدكتور
            .FirstOrDefaultAsync(c => c.ClinicId == clinicId && c.DoctorId == doctorId);

        if (clinic == null)
            throw new Exception("العيادة مش موجودة أو مش بتاعتك");

        if (request.WorkingDays != null && request.WorkStartTime != null && request.WorkEndTime != null)
        {
            await ValidateNoOverlapAsync(doctorId, clinicId, request);
        }

        if (!string.IsNullOrWhiteSpace(request.ClinicName)) clinic.ClinicName = request.ClinicName;
        if (!string.IsNullOrWhiteSpace(request.ClinicPhone)) clinic.ClinicPhone = request.ClinicPhone;
        if (!string.IsNullOrWhiteSpace(request.ClinicEmail)) clinic.ClinicEmail = request.ClinicEmail;
        if (!string.IsNullOrWhiteSpace(request.Location)) clinic.Location = request.Location;
        if (!string.IsNullOrWhiteSpace(request.WorkingDays)) clinic.WorkingDays = request.WorkingDays;
        if (request.DoctorPrice.HasValue) clinic.DoctorPrice = request.DoctorPrice.Value;
        if (request.SlotDurationMinutes.HasValue) clinic.SlotDurationMinutes = request.SlotDurationMinutes.Value;

        if (!string.IsNullOrWhiteSpace(request.WorkStartTime))
            clinic.WorkStartTime = ParseTime(request.WorkStartTime);

        if (!string.IsNullOrWhiteSpace(request.WorkEndTime))
            clinic.WorkEndTime = ParseTime(request.WorkEndTime);

        await _context.SaveChangesAsync();
        return MapToResponse(clinic);
    }

    // ================================================================
    // حذف عيادة
    // ================================================================
    public async Task<bool> DeleteClinicAsync(int doctorId, int clinicId)
    {
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.ClinicId == clinicId && c.DoctorId == doctorId);

        if (clinic == null)
            throw new Exception("العيادة مش موجودة أو مش بتاعتك");

        _context.Clinics.Remove(clinic);
        await _context.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // إضافة بريك لعيادة
    // ================================================================
    public async Task<ClinicResponse> AddBreakAsync(int doctorId, int clinicId, AddBreakRequest request)
    {
        var clinic = await _context.Clinics
            .Include(c => c.Breaks)
            .Include(c => c.Doctor) // ✅ زودنا Include للدكتور
            .FirstOrDefaultAsync(c => c.ClinicId == clinicId && c.DoctorId == doctorId);

        if (clinic == null)
            throw new Exception("العيادة مش موجودة أو مش بتاعتك");

        if (!TimeOnly.TryParse(request.BreakTime, out var breakTime))
            throw new Exception("الوقت مش صح، استخدم format زي 13:00");

        if (clinic.WorkStartTime == null || clinic.WorkEndTime == null)
            throw new Exception("لازم تحدد مواعيد العيادة الأول");

        var start = TimeOnly.FromTimeSpan(clinic.WorkStartTime.Value);
        var end = TimeOnly.FromTimeSpan(clinic.WorkEndTime.Value);
        var slotMins = clinic.SlotDurationMinutes;

        bool isValidSlot = false;
        var current = start;
        while (current.AddMinutes(slotMins) <= end)
        {
            if (current == breakTime) { isValidSlot = true; break; }
            current = current.AddMinutes(slotMins);
        }

        if (!isValidSlot)
            throw new Exception("الوقت ده مش من ضمن المواعيد المتاحة");

        bool exists = clinic.Breaks.Any(b => b.BreakTime == breakTime);
        if (exists)
            throw new Exception("البريك ده موجود بالفعل");

        clinic.Breaks.Add(new ClinicBreak { ClinicId = clinicId, BreakTime = breakTime });
        await _context.SaveChangesAsync();

        return MapToResponse(clinic);
    }

    // ================================================================
    // حذف بريك
    // ================================================================
    public async Task<bool> DeleteBreakAsync(int doctorId, int clinicId, string breakTime)
    {
        if (!TimeOnly.TryParse(breakTime, out var time))
            throw new Exception("الوقت مش صح");

        var breakEntry = await _context.ClinicBreaks
            .Include(b => b.Clinic)
            .FirstOrDefaultAsync(b => b.ClinicId == clinicId
                                   && b.Clinic.DoctorId == doctorId
                                   && b.BreakTime == time);

        if (breakEntry == null)
            throw new Exception("البريك مش موجود");

        _context.ClinicBreaks.Remove(breakEntry);
        await _context.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // Helper: تحقق من تضارب المواعيد
    // ================================================================
    private async Task ValidateNoOverlapAsync(int doctorId, int? excludeClinicId, ClinicUpsertRequest request)
    {
        var newDays = request.WorkingDays!.Split(',').Select(d => d.Trim()).ToList();
        var newStart = TimeOnly.Parse(request.WorkStartTime!);
        var newEnd = TimeOnly.Parse(request.WorkEndTime!);

        var otherClinics = await _context.Clinics
            .Where(c => c.DoctorId == doctorId
                     && c.WorkingDays != null
                     && c.WorkStartTime != null
                     && c.WorkEndTime != null
                     && (excludeClinicId == null || c.ClinicId != excludeClinicId))
            .ToListAsync();

        foreach (var clinic in otherClinics)
        {
            var existingDays = clinic.WorkingDays!.Split(',').Select(d => d.Trim());
            var existingStart = TimeOnly.FromTimeSpan(clinic.WorkStartTime!.Value);
            var existingEnd = TimeOnly.FromTimeSpan(clinic.WorkEndTime!.Value);

            bool hasSameDay = newDays.Any(d => existingDays.Contains(d));
            if (!hasSameDay) continue;

            bool overlap = newStart < existingEnd && newEnd > existingStart;
            if (overlap)
                throw new Exception($"المواعيد بتتضارب مع عيادة {clinic.ClinicName} — الدكتور شغال في نفس الوقت");
        }
    }

    // ================================================================
    // Helper: Parse time string
    // ================================================================
    private TimeSpan? ParseTime(string? time)
    {
        if (string.IsNullOrWhiteSpace(time)) return null;
        return TimeSpan.TryParse(time, out var result) ? result : null;
    }

    // ================================================================
    // Helper: Map to Response
    // ================================================================
    private ClinicResponse MapToResponse(Clinic clinic)
    {
        return new ClinicResponse
        {
            ClinicId = clinic.ClinicId,
            DoctorName = clinic.Doctor?.DoctorName, // ✅ زودنا اسم الدكتور
            ClinicName = clinic.ClinicName,
            ClinicPhone = clinic.ClinicPhone,
            ClinicEmail = clinic.ClinicEmail,
            Location = clinic.Location,
            WorkingDays = clinic.WorkingDays,
            WorkStartTime = clinic.WorkStartTime?.ToString(@"hh\:mm"),
            WorkEndTime = clinic.WorkEndTime?.ToString(@"hh\:mm"),
            DoctorPrice = clinic.DoctorPrice,
            SlotDurationMinutes = clinic.SlotDurationMinutes,
            Breaks = clinic.Breaks.Select(b => b.BreakTime.ToString("HH:mm")).ToList()
        };
    }
}