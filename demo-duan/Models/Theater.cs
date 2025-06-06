using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Theater
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Theater name is required")]
        [StringLength(200, ErrorMessage = "Theater name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;
        
        // Navigation properties - ĐẢM BẢO DÒNG NÀY CÓ
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}