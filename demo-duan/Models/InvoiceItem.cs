using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        
        [Required]
        public int InvoiceId { get; set; }
        
        public int? TicketId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public int Quantity { get; set; } = 1;
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;
        public virtual Ticket? Ticket { get; set; }
    }
}