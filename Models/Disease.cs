
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Healthcare.Models;

[Table("diseases")]

public partial class Disease

{

    [Key]

    [Column("disease_id")]

    public int DiseaseId { get; set; }

    [Column("diseases_name")]

    [StringLength(100)]

    public string? DiseasesName { get; set; }




    [Column("disease_severity")]

    [StringLength (20)]

    public string? disease_severity { get; set; }


  


    [Column("specialty_id")]

    public int? SpecialtyId { get; set; }

    [ForeignKey("SpecialtyId")]

    public virtual Specialty? Specialty { get; set; }

   

}