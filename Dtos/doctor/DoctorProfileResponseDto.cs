using Microsoft.AspNetCore.Http;

namespace Healthcare.Dtos.doctor
{
    public class DoctorProfileResponseDto
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorCity { get; set; }
        public string? DoctorPhone { get; set; }
        public string? DoctorEmail { get; set; }
        public decimal? DoctorPrice { get; set; }
        public string? Certificate { get; set; }
        public string? LicenseNumber { get; set; }
        public string? DoctorImageUrl { get; set; }
        public long? NationalNumber { get; set; }
        public string? WorkingDays { get; set; }
        public string? WorkStartTime { get; set; }
        public string? WorkEndTime { get; set; }
        // ✅ شلنا WorkHoursTotal لأن الـ computed column اتحذفت من الـ DB
        public decimal? AverageRating { get; set; }
        public int? SpecialtyId { get; set; }
        public string? SpecialtyName { get; set; }
        public int? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicPhone { get; set; }
        public string? ClinicEmail { get; set; }
        public string? ClinicLocation { get; set; }
    }

    public class DoctorProfileUpdateDto
    {
        public string? DoctorName { get; set; }
        public string? DoctorPhone { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorCity { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}