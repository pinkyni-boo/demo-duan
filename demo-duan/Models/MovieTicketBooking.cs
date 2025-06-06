using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class MovieTicketBooking
    {
        [Required]
        [Display(Name = "Movie Name")]
        public string MovieName { get; set; }

        [Required]
        [Display(Name = "Showtime")]
        public DateTime Showtime { get; set; }

        [Required]
        [Display(Name = "Seat Number")]
        public string Seat { get; set; }

        [Required]
        [Display(Name = "Your Name")]
        public string CustomerName { get; set; }
    }
}
