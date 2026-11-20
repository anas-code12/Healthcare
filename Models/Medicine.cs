using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Healthcare.Models;

[Table("medicine")]
public partial class Medicine
{
    [Key]
    [Column("medicine_id")]
    public int MedicineId { get; set; }

    [Column("medicine_name")]
    [StringLength(100)]
    public string? MedicineName { get; set; }

    [Column("specialty_id")]
    public int? SpecialtyId { get; set; }

    [ForeignKey("SpecialtyId")]
    public virtual Specialty? Specialty { get; set; }
}