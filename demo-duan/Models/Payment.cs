using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public int TicketId { get; set; }
        
        [Required]
        public int PaymentMethodId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        
        [StringLength(100)]
        public string? TransactionId { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    }
}