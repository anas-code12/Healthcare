namespace Healthcare.Dtos.Patient
{
    public class PatientSickRecordUpsertRequest
    {
        public int patient_id { get; set; }

        public int? pationt_Hight { get; set; }
        public int? pationt_weight { get; set; }

        public bool? patient_anaemia { get; set; }
        public bool? patient_hypertension { get; set; }
        public bool? patient_diabetes { get; set; }

        // Urban = true , Rural = false  (زي ما اتفقنا قبل كده)
        public string? residence_type { get; set; } // "Urban" / "Rural"

        public string? patient_bloodtype { get; set; } // A+, O- ...

  
    }
}
