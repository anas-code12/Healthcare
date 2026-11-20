namespace Healthcare.Dtos.Clinics;

// ============================================
// Request: إضافة أو تعديل عيادة
// ============================================
public class ClinicUpsertRequest
{
    
    public string? ClinicName { get; set; }
    public string? ClinicPhone { get; set; }
    public string? ClinicEmail { get; set; }
    public string? Location { get; set; }

    // Format: "Saturday,Sunday,Monday"
    public string? WorkingDays { get; set; }
    public string? WorkStartTime { get; set; }   // "09:00"
    public string? WorkEndTime { get; set; }     // "17:00"
    public decimal? DoctorPrice { get; set; }
    public int? SlotDurationMinutes { get; set; } // مدة الكشف بالدقائق
}


public class AddBreakRequest
{
    public string BreakTime { get; set; } = ""; // "13:00"
}

public class ClinicResponse
{
    public string? DoctorName { get; set; }
    public int ClinicId { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicPhone { get; set; }
    public string? ClinicEmail { get; set; }
    public string? Location { get; set; }
    public string? WorkingDays { get; set; }
    public string? WorkStartTime { get; set; }
    public string? WorkEndTime { get; set; }
    public decimal? DoctorPrice { get; set; }
    public int SlotDurationMinutes { get; set; }
    public List<string> Breaks { get; set; } = new(); // ["13:00", "13:30"]
}