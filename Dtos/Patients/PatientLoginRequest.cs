namespace Healthcare.Dtos.Patient
{
    public class PatientLoginRequest
    {
        public string patient_email { get; set; } = null!;
        public string patient_password { get; set; } = null!;
    }
}
