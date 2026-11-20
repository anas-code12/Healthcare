using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("specialty")]
public partial class Specialty
{
    [Key]
    [Column("specialty_id")]
    public int SpecialtyId { get; set; }

    [Column("specialty_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? SpecialtyName { get; set; }

    [InverseProperty("Specialty")]
    public virtual ICollection<Disease> Diseases { get; set; } = new List<Disease>();
}
