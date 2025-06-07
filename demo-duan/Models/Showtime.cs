using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Showtime
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        public int TheaterId { get; set; }
        
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        
        [Range(0, 500)]
        public int AvailableSeats { get; set; }
        
        [Range(0, 500)]
        public int TotalSeats { get; set; } = 100;
        
        // Navigation Properties
        public virtual Movie Movie { get; set; } = null!;
        public virtual Theater Theater { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
