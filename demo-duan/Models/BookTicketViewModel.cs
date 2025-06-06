using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class BookTicketViewModel
    {
        [Required]
        public int ShowtimeId { get; set; }
        
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Customer Name")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please select seat(s)")]
        [Display(Name = "Selected Seats")]
        public string Seat { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        public int Quantity { get; set; } = 1;
    }
}