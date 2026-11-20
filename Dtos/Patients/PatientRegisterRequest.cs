using System.ComponentModel.DataAnnotations;

namespace Healthcare.Dtos.Patient
{
    public class PatientRegisterRequest
    {
        public string patient_name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@]+@gmail\.com$",
            ErrorMessage = "الإيميل لازم يكون @gmail.com")]
        public string patient_email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,}$",
            ErrorMessage = "الباسورد لازم يكون 8 حروف على الأقل ويحتوي على حروف وأرقام")]
        public string patient_password { get; set; } = null!;

        public string? patient_phone { get; set; }
        public string? Patient_City { get; set; }
        public string? patient_Gender { get; set; }
        public bool? patient_married { get; set; }
        public DateOnly? birth_date { get; set; }
    }
}
