namespace Healthcare.Dtos.doctor
{
    public class SendMessageRequest
    {
        public string MessageBody { get; set; } = null!;
    }

    public class MinistryToDocterMessageRequest
    {
        public int DoctorId { get; set; }
        public string MessageBody { get; set; } = null!;
    }

    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? MessageBody { get; set; }
        public string? Sender { get; set; }
        public string? SpecialtyName { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class MinistryLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class VerifyDoctorRequest
    {
        public string LicenseNumber { get; set; } = null!;
        public bool IsVerified { get; set; }
    }

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
}