using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        public int ShowtimeId { get; set; }
        
        public int? MovieId { get; set; }
        
        public string? UserId { get; set; }
        
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(256, ErrorMessage = "Name cannot exceed 256 characters")]
        [Display(Name = "Customer Name")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256)]
        public string UserEmail { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Seat is required")]
        [StringLength(10, ErrorMessage = "Seat cannot exceed 10 characters")]
        public string Seat { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        [StringLength(200)]
        public string MovieTitle { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string CinemaName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string UserRole { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled, Used
        
        [Required]
        [Display(Name = "Booking Date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Showtime Showtime { get; set; } = null!;
        public virtual Movie? Movie { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
