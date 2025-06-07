using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        // Navigation Property
        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}