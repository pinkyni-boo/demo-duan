using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class MovieTicketBooking
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Movie Name")]
        public required string MovieName { get; set; }
        
        [Required]
        [Display(Name = "Show Date")]
        public DateTime ShowDate { get; set; }
        
        [Required]
        [Display(Name = "Show Time")]
        public TimeSpan ShowTime { get; set; }
        
        [Required]
        [Display(Name = "Seat Number")]
        public required string Seat { get; set; }
        
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        
        [Required]
        [Display(Name = "Your Name")]
        public required string CustomerName { get; set; }
        
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }
        
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }
    }
}
