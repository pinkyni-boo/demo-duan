using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}