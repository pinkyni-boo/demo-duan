using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace demo_duan.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [Range(1, 500)]
        public int SeatNumber { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Booked";

        [StringLength(20)]
        public string TicketCode { get; set; } = null!;

        // Navigation properties
        public virtual Showtime Showtime { get; set; } = null!;
        public virtual Movie Movie { get; set; } = null!;
        public virtual IdentityUser User { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
