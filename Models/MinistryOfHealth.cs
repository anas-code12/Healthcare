using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("Ministry_of_Health")]
[Index("MinistryOfHealthEmail", Name = "UX_moh_email", IsUnique = true)]
public partial class MinistryOfHealth
{
    [Key]
    [Column("Ministry_of_Health_id")]
    public int MinistryOfHealthId { get; set; }

    [Column("Ministry_of_Health_email")]
    [StringLength(100)]
    [Unicode(false)]
    public string? MinistryOfHealthEmail { get; set; }

    [Column("Ministry_of_Health_password")]
    [StringLength(255)]
    [Unicode(false)]
    public string MinistryOfHealthPassword { get; set; } = null!;

    [Column("Ministry_of_Health_message")]
    [Unicode(false)]
    public string? MinistryOfHealthMessage { get; set; }
}
