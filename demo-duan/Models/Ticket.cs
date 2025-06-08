using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Booked"; // Booked, Cancelled, Used

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Computed property - for backward compatibility
        [NotMapped]
        public decimal TotalPrice => Price;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Movie Movie { get; set; } = null!;
        public virtual Showtime Showtime { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
