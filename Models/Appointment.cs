using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Healthcare.Models
{
    [Table("appointment")]
    public partial class Appointment
    {
        [Key]
        [Column("appointment_id")]
        public int AppointmentId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("clinic_id")]
        public int ClinicId { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string? Status { get; set; }

        [Column("appointment_date")]
        public DateOnly? AppointmentDate { get; set; }

        [Column("appointment_time")]
        public TimeOnly? AppointmentTime { get; set; }

        [Column("duration_minutes")]
        public int? DurationMinutes { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [Column("notes")]
        [StringLength(255)]
        public string? Notes { get; set; }

        [Column("diseases_id")]
        [StringLength(500)]
        public string? DiseasesId { get; set; }

        [Column("medicines_json")]
        public string? MedicinesJson { get; set; }

        [Column("disease_Description")]
        [StringLength(500)]
        public string? DiseaseDescription { get; set; }

        [Column("ai_diagnosis")]
        [StringLength(1000)]
        public string? AiDiagnosis { get; set; }

        [Column("patient_rating")]
        public int? PatientRating { get; set; }

        // Navigation Properties
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }
    }
}