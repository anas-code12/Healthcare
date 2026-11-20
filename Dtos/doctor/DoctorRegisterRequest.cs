using System.ComponentModel.DataAnnotations;

namespace Healthcare.Dtos.doctor
{
    public class DoctorRegisterRequest
    {
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@]+@gmail\.com$",
            ErrorMessage = "الإيميل لازم يكون @gmail.com")]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,}$",
            ErrorMessage = "الباسورد لازم يكون 8 حروف على الأقل ويحتوي على حروف وأرقام")]
        public string Password { get; set; } = null!;

        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? LicenseNumber { get; set; }
        public long? NationalNumber { get; set; }
        public string? Certificate { get; set; }
        public string? SpecialtyName { get; set; }
    }
}