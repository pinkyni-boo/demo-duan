using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Theater
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Range(1, 1000)]
        public int Capacity { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(300)]
        public string? Address { get; set; }
        
        [StringLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        // Navigation Properties
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}