public class AddDiseaseRequest
{
    public string DiseasesName { get; set; } = null!;
    public string? DiseaseSeverity { get; set; }
    public int SpecialtyId { get; set; }
}

public class AddMedicineRequest
{
    public string MedicineName { get; set; } = null!;
    public int SpecialtyId { get; set; }
}