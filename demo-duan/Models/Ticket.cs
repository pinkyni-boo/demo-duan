using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        public int ShowtimeId { get; set; }
        
        public int MovieId { get; set; }
        
        public int BookingId { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string SeatCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Booked"; // Booked, Confirmed, Cancelled
        
        [Required]
        [StringLength(20)]
        public string TicketCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Showtime? Showtime { get; set; }
        public virtual Movie? Movie { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Booking? Booking { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual ICollection<Invoice>? Invoices { get; set; }
    }
}
