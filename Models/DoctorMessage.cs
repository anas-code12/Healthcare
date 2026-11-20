using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Healthcare.Models
{
    [Table("doctor_messages")]
    public partial class DoctorMessage
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("message_body")]
        [StringLength(1000)]
        public string MessageBody { get; set; } = null!;

        [Column("sender")]
        [StringLength(20)]
        public string Sender { get; set; } = "Doctor"; // Doctor or Ministry

        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
        [Column("specialty_id")]
        public int? SpecialtyId { get; set; }

        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }
    }
}