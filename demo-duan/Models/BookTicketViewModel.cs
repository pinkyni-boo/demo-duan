using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class BookTicketViewModel
    {
        [Required]
        public int ShowtimeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public string SeatNumber { get; set; } = string.Empty; // "A1,A2,A3" for multiple seats
        
        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}