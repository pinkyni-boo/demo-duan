using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }

    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 16);

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        
        // Add ProcessedDate property
        public DateTime? ProcessedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    }
}