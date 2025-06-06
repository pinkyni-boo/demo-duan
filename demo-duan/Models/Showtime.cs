using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Showtime
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please select a movie")]
        [Display(Name = "Movie")]
        public int MovieId { get; set; }
        
        [Required(ErrorMessage = "Please select a theater")]
        [Display(Name = "Theater")]
        public int TheaterId { get; set; }
        
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Time is required")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Available seats must be non-negative")]
        [Display(Name = "Available Seats")]
        public int AvailableSeats { get; set; }
        
        // Navigation properties
        public virtual Movie Movie { get; set; } = null!;
        public virtual Theater Theater { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
