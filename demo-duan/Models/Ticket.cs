using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        public int MovieId { get; set; }
        public int ShowtimeId { get; set; }
        
        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty; // A1, B2, C3, etc.
        
        [StringLength(50)]
        public string BookingReference { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Booked"; // Booked, Cancelled, Used
        
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        // Navigation Properties
        public virtual Movie Movie { get; set; } = null!;
        public virtual Showtime Showtime { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
