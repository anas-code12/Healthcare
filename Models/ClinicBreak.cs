using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("clinic_breaks")]
public class ClinicBreak
{
    [Key]
    [Column("break_id")]
    public int BreakId { get; set; }

    [Column("clinic_id")]
    public int ClinicId { get; set; }

    [Column("break_time")]
    public TimeOnly BreakTime { get; set; }

    [ForeignKey("ClinicId")]
    [InverseProperty("Breaks")]
    public virtual Clinic Clinic { get; set; } = null!;
}