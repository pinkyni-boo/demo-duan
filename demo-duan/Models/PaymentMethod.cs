using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Credit Card, PayPal, Bank Transfer, etc.
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string? Icon { get; set; } // CSS class or image path
        
        // Navigation property
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}