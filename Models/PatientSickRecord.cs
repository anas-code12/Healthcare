using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

[Table("patient_Sick_record")]
[Index("PatientId", Name = "UQ__patient___4D5CE4772D93F13F", IsUnique = true)]
public partial class PatientSickRecord
{
    [Key]
    [Column("Sick_record_id")]
    public int SickRecordId { get; set; }

    [Column("pationt_Hight")]
    public int? PationtHight { get; set; }

    [Column("pationt_weight")]
    public int? PationtWeight { get; set; }

    [Column("patient_anaemia")]
    public bool? PatientAnaemia { get; set; }

    [Column("patient_hypertension")]
    public bool? PatientHypertension { get; set; }

    [Column("patient_diabetes")]
    public bool? PatientDiabetes { get; set; }

    [Column("Residence_type")]
    public bool? ResidenceType { get; set; }

    [Column("patient_bloodtype")]
    [StringLength(5)]
    [Unicode(false)]
    public string? PatientBloodtype { get; set; }

    [Column("patient_id")]
    public int PatientId { get; set; }



    [Column("notes")]
    [StringLength(500)]
    [Unicode(false)]
    public string? Notes { get; set; }


    [ForeignKey("PatientId")]
    [InverseProperty("PatientSickRecord")]
    public virtual Patient Patient { get; set; } = null!;
}
