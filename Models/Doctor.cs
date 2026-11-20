using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("doctors")]
[Index("DoctorEmail", Name = "UX_doctors_email", IsUnique = true)]
public partial class Doctor
{
    [Key]
    [Column("doctor_id")]
    public int DoctorId { get; set; }

    [Column("doctor_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? DoctorName { get; set; }

    [Column("doctor_city")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DoctorCity { get; set; }

    [Column("doctor_phon")]
    [StringLength(11)]
    [Unicode(false)]
    public string? DoctorPhon { get; set; }

    [Column("doctor_email")]
    [StringLength(100)]
    [Unicode(false)]
    public string? DoctorEmail { get; set; }

    [Column("doctor_password")]
    [StringLength(255)]
    [Unicode(false)]
    public string DoctorPassword { get; set; } = null!;

    [Column("certificate")]
    [Unicode(false)]
    public string? Certificate { get; set; }

  

    [Column("doctor_imge")]
    [Unicode(false)]
    public string? DoctorImge { get; set; }

    [Column("License_Number")]
    [StringLength(20)]
    [Unicode(false)]
    public string? LicenseNumber { get; set; }

    [Column("national_number")]
    public long? NationalNumber { get; set; }

   
    

  
   

    // ✅ شلنا work_hours_total لأنها اتحذفت من الـ DB

    [Column("average_rating", TypeName = "decimal(3, 2)")]
    public decimal? AverageRating { get; set; }

    [Column("total_ratings")]
    public int? TotalRatings { get; set; }

    [Column("specialty_id")]
    public int? SpecialtyId { get; set; }

    // ✅ مدة الكشف الواحد بالدقائق - افتراضي 30
   

    // Navigation Properties
    [ForeignKey("SpecialtyId")]
    public virtual Specialty? Specialty { get; set; }

    [InverseProperty("Doctor")]
    public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();

   
    [InverseProperty("Doctor")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [InverseProperty("Doctor")]
    public virtual ICollection<DoctorMessage> DoctorMessages { get; set; } = new List<DoctorMessage>();
}
