using Healthcare.Api.Dtos;
using Healthcare.Dtos;
using Healthcare.Dtos.Patient;
using Healthcare.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Services.Patients;

public class PatientService
{
    private readonly HealthcareSystemContext _db;

    public PatientService(HealthcareSystemContext db)
    {
        _db = db;
    }

    // ================================================================
    // Upsert Sick Record
    // ================================================================
    public async Task<AuthResponse> UpsertSickRecordAsync(PatientSickRecordUpsertRequest req)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == req.patient_id);
        if (patient == null)
            return new AuthResponse { Success = false, Message = "Patient not found." };

        bool? residenceBit = null;
        if (!string.IsNullOrWhiteSpace(req.residence_type))
        {
            var r = req.residence_type.Trim().ToLower();
            residenceBit = (r == "urban") ? true :
                           (r == "rural") ? false : null;
        }

        var record = await _db.PatientSickRecords.FirstOrDefaultAsync(x => x.PatientId == req.patient_id);
        if (record == null)
        {
            record = new PatientSickRecord { PatientId = req.patient_id };
            _db.PatientSickRecords.Add(record);
        }

        record.PationtHight = req.pationt_Hight;
        record.PationtWeight = req.pationt_weight;
        record.PatientAnaemia = req.patient_anaemia;
        record.PatientHypertension = req.patient_hypertension;
        record.PatientDiabetes = req.patient_diabetes;
        record.ResidenceType = residenceBit;
        record.PatientBloodtype = req.patient_bloodtype;

        await _db.SaveChangesAsync();
        return new AuthResponse { Success = true, Message = "Sick record saved." };
    }

    // ================================================================
    // Update Patient Profile
    // ================================================================
    public async Task<AuthResponse> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileRequest req)
    {
        var patient = await _db.Patients
            .Include(p => p.PatientSickRecord)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (patient == null)
            return new AuthResponse { Success = false, Message = "Patient not found." };

        // تحديث بيانات المريض
        if (req.PatientName != null) patient.PatientName = req.PatientName;
        if (req.PatientPhone != null) patient.PatientPhone = req.PatientPhone;
        if (req.PatientCity != null) patient.PatientCity = req.PatientCity;
        if (req.PatientGender != null)
        {
            var g = req.PatientGender.Trim().ToLower();
            patient.PatientGender = g == "male" ? "M" :
                                    g == "female" ? "F" :
                                    g == "m" ? "M" :
                                    g == "f" ? "F" : null;
        }
        if (req.PatientMarried != null) patient.PatientMarried = req.PatientMarried;

        // حساب العمر من تاريخ الميلاد
        if (!string.IsNullOrWhiteSpace(req.BirthDate))
        {
            if (DateOnly.TryParse(req.BirthDate, out var birthDate))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;
                patient.PatientAge = age;
            }
        }

        // تحديث السجل الطبي
        var record = patient.PatientSickRecord;
        if (record == null)
        {
            record = new PatientSickRecord { PatientId = patientId };
            _db.PatientSickRecords.Add(record);
        }

        if (req.PatientHeight != null) record.PationtHight = req.PatientHeight;
        if (req.PatientWeight != null) record.PationtWeight = req.PatientWeight;
        if (req.PatientBloodtype != null) record.PatientBloodtype = req.PatientBloodtype;
        if (req.PatientAnemia != null) record.PatientAnaemia = req.PatientAnemia;
        if (req.PatientHypertension != null) record.PatientHypertension = req.PatientHypertension;
        if (req.PatientDiabetes != null) record.PatientDiabetes = req.PatientDiabetes;

        await _db.SaveChangesAsync();
        return new AuthResponse { Success = true, Message = "تم تحديث البيانات بنجاح." };
    }
}