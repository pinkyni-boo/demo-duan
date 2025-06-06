using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public int TicketId { get; set; }
        
        public string? UserId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Credit Card, Debit Card, Cash, etc.
        
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string PaymentGateway { get; set; } = string.Empty; // PayPal, Stripe, VNPay, etc.
        
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty; // Last 4 digits only
        
        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string BankName { get; set; } = string.Empty;
        
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}