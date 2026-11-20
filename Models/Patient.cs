using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models
{
    [Table("patients")]
    [Index("PatientEmail", Name = "UX_patients_email", IsUnique = true)]
    public partial class Patient
    {
        [Key]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("patient_name")]
        [StringLength(100)]
        [Unicode(false)]
        public string? PatientName { get; set; }

        [Column("patient_age")]
        public int? PatientAge { get; set; }

        [Column("patient_gender")]
        [StringLength(1)]
        [Unicode(false)]
        public string? PatientGender { get; set; }

        [Column("patient_email")]
        [StringLength(100)]
        [Unicode(false)]
        public string? PatientEmail { get; set; }

        [Column("patient_password")]
        [StringLength(255)]
        [Unicode(false)]
        public string PatientPassword { get; set; } = null!;

        [Column("patient_married")]
        public bool? PatientMarried { get; set; }

        [Column("patient_phone")]
        [StringLength(11)]
        [Unicode(false)]
        public string? PatientPhone { get; set; }

        [Column("patient_city")]
        [StringLength(50)]
        [Unicode(false)]
        public string? PatientCity { get; set; }


        [InverseProperty("Patient")]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [InverseProperty("Patient")]
        public virtual PatientSickRecord? PatientSickRecord { get; set; }
    }
}