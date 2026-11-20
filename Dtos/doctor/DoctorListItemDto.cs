namespace Healthcare.Dtos.doctor
{
    public class DoctorListItemDto
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? SpecialtyName { get; set; }
        public decimal? AverageRating { get; set; }
        public string? DoctorCity { get; set; }
        public string? ClinicPhone { get; set; }
        public string? WorkingDays { get; set; }
        public string? WorkStartTime { get; set; }
        public string? WorkEndTime { get; set; }
        public decimal? WorkHoursTotal { get; set; }
        public decimal? DoctorPrice { get; set; }
        public string? DoctorImageUrl { get; set; }
    }
}