namespace Healthcare.Dtos.Appointments;

public class BookAppointmentRequest
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
}

public class SlotDto
{
    public string Time { get; set; } = "";
    public bool Available { get; set; }
}

public class DaySlots
{
    public string Date { get; set; } = "";
    public List<SlotDto> Slots { get; set; } = new();
}

public class DoctorAvailabilityResponse
{
    public int DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public int ClinicId { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicPhone { get; set; }
    public string? ClinicAddress { get; set; }
    public decimal? DoctorPrice { get; set; }
    public List<string> WorkingDays { get; set; } = new();
    public List<DaySlots> AvailableSlots { get; set; } = new();
}

// ✅ كل دواء وجرعته
public class MedicineDetailDto
{
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public string? Dose { get; set; }
    public string? Form { get; set; }
    public string? Frequency { get; set; }
    public int? Days { get; set; }
    public string? Timing { get; set; }
}

// ✅ Request بتاع الدكتور لما بيكمل الكشف
public class MedicineRequest
{
    public int MedicineId { get; set; }
    public string? Dose { get; set; }
    public string? Form { get; set; }
    public string? Frequency { get; set; }
    public int? Days { get; set; }
    public string? Timing { get; set; }
}

// ✅ للمريض - بيشوف بيانات الدكتور
public class AppointmentResponse
{
    public int AppointmentId { get; set; }
    public int? DoctorId { get; set; }
    public int? ClinicId { get; set; }
    public string? DoctorName { get; set; }
    public string? Specialty { get; set; }
    public decimal? Rating { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? AppointmentTime { get; set; }
    public string? Status { get; set; }
    public int? PatientRating { get; set; }

    // ✅ Completed فقط
    public List<string>? DiseaseNames { get; set; }
    public List<MedicineDetailDto>? Medicines { get; set; }
    public string? DiseaseDescription { get; set; }
    public string? Notes { get; set; }
}

// ✅ للدكتور - بيشوف بيانات المرضى
public class DoctorAppointmentResponse
{
    public int AppointmentId { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? ClinicName { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? AppointmentTime { get; set; }
    public string? Status { get; set; }
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? PatientCity { get; set; }
    public bool? PatientMarried { get; set; }
    public string? PatientPhone { get; set; }
    public int? PatientHeight { get; set; }
    public int? PatientWeight { get; set; }
    public bool? PatientAnaemia { get; set; }
    public bool? PatientHypertension { get; set; }
    public bool? PatientDiabetes { get; set; }
    public string? PatientBloodtype { get; set; }
}

// ✅ الدكتور يكشف على المريض
public class CompleteAppointmentRequest
{
    public string? Notes { get; set; }
    public string? DiseaseDescription { get; set; }
    public List<int>? DiseasesIds { get; set; }
    public List<MedicineRequest>? Medicines { get; set; }
}

// AI Team بيبعت الرد
public class AiDiagnosisRequest
{
    public int AppointmentId { get; set; }
    public string? Diagnosis { get; set; }
}

// وزارة الصحة بتشوف النتايج
public class HealthMinistryResponse
{
    public int AppointmentId { get; set; }
    public string? DiseaseDescription { get; set; }
    public string? AiDiagnosis { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? PatientName { get; set; }
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? PatientCity { get; set; }
    public string? PatientPhone { get; set; }
    public bool? PatientMarried { get; set; }
}

public class DiseaseCityCount
{
    public string? City { get; set; }
    public int Count { get; set; }
}

public class ChronicDiseaseStatsResponse
{
    public string? DiseaseName { get; set; }
    public int TotalPatients { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public List<DiseaseCityCount> ByCity { get; set; } = new();
}

public class RateAppointmentRequest
{
    public int Rating { get; set; }
}

public class PatientMedicalHistoryResponse
{
    public int AppointmentId { get; set; }
    public string? DoctorName { get; set; }
    public string? Specialty { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? AppointmentTime { get; set; }
    public List<string>? DiseaseNames { get; set; }
    public List<MedicineDetailDto>? Medicines { get; set; }
    public string? DiseaseDescription { get; set; }
    public string? Notes { get; set; }
}


