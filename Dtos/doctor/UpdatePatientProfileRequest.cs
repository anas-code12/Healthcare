namespace Healthcare.Dtos.Patient
{
    public class UpdatePatientProfileRequest
    {
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
        public string? PatientCity { get; set; }
        public string? PatientGender { get; set; }
        public bool? PatientMarried { get; set; }
        public string? BirthDate { get; set; }
        public int? PatientHeight { get; set; }
        public int? PatientWeight { get; set; }
        public string? PatientBloodtype { get; set; }
        public bool? PatientAnemia { get; set; }
        public bool? PatientHypertension { get; set; }
        public bool? PatientDiabetes { get; set; }
    }
}