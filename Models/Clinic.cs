using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("clinics")]
public partial class Clinic
{
    [Key]
    [Column("clinic_id")]
    public int ClinicId { get; set; }

    [Column("clinic_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? ClinicName { get; set; }

    [Column("clinic_phone")]
    [StringLength(20)]
    [Unicode(false)]
    public string? ClinicPhone { get; set; }

    [Column("clinic_email")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ClinicEmail { get; set; }

    [Column("location")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Location { get; set; }

    [Column("doctor_id")]
    public int? DoctorId { get; set; }

    // ✅ جديد - مواعيد وبيانات العيادة
    [Column("working_days")]
    [StringLength(100)]
    [Unicode(false)]
    public string? WorkingDays { get; set; }

    [Column("work_start_time")]
    public TimeSpan? WorkStartTime { get; set; }

    [Column("work_end_time")]
    public TimeSpan? WorkEndTime { get; set; }

    [Column("doctor_price", TypeName = "decimal(8, 2)")]
    public decimal? DoctorPrice { get; set; }

    [Column("slot_duration_minutes")]
    public int SlotDurationMinutes { get; set; } = 30;

    // Navigation Properties
    [ForeignKey("DoctorId")]
    [InverseProperty("Clinics")]
    public virtual Doctor? Doctor { get; set; }

    [InverseProperty("Clinic")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    // ✅ جديد - البريك بتاع العيادة
    [InverseProperty("Clinic")]
    public virtual ICollection<ClinicBreak> Breaks { get; set; } = new List<ClinicBreak>();
}