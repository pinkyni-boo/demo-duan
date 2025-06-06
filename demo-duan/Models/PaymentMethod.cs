using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Credit Card, Debit Card, PayPal, etc.
        
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public decimal ProcessingFee { get; set; } = 0; // Phí xử lý
        
        public string IconUrl { get; set; } = string.Empty; // URL icon của payment method
        
        // Navigation properties
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}