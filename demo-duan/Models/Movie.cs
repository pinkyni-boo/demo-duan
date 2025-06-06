using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 minutes")]
        public int Duration { get; set; }
        
        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; }
        
        public string Img { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        
        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
