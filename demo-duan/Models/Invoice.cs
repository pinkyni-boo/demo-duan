using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public int? PaymentId { get; set; }
        
        public decimal SubTotal { get; set; }
        
        public decimal Tax { get; set; }
        
        public decimal Discount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Cancelled
        
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [StringLength(256)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        
        public DateTime DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Payment? Payment { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}