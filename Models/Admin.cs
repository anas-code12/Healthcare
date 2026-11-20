using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("admin")]
[Index("AdminEmail", Name = "UX_admin_email", IsUnique = true)]
public partial class Admin
{
    [Key]
    [Column("admin_id")]
    public int AdminId { get; set; }

    [Column("admin_email")]
    [StringLength(100)]
    [Unicode(false)]
    public string? AdminEmail { get; set; }

    [Column("admin_password")]
    [StringLength(255)]
    [Unicode(false)]
    public string AdminPassword { get; set; } = null!;

    [Column("admin_message")]
    [Unicode(false)]
    public string? AdminMessage { get; set; }
}
