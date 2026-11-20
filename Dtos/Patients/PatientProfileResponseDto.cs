namespace Healthcare.Dtos.patient
{
    public class PatientProfileResponseDto
    {
        public int Patient_id { get; set; }
        public string? Patient_name { get; set; }
        public int? Patient_age { get; set; }
        public string? Patient_Gender { get; set; }
        public string? Patient_city { get; set; }
        public string? Patient_email { get; set; }
        public bool Patient_married { get; set; }
        public string? Patient_phone { get; set; }
       
        // ✅ من جدول PatientSickRecord
        public int? Patient_height { get; set; }
        public int? Patient_weight { get; set; }
        public bool? Patient_anemia { get; set; }
        public bool? Patient_hypertension { get; set; }
        public bool? Patient_diabetes { get; set; }
        public string? Patient_bloodtype { get; set; }
    }
}