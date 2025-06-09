using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BookingCode { get; set; } = string.Empty;
        
        public int ShowtimeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending";
        
        public bool IsActive { get; set; } = true;
        
        public string? UserId { get; set; }
        
        // Navigation properties
        public virtual Showtime? Showtime { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Ticket>? Tickets { get; set; }
    }
}
