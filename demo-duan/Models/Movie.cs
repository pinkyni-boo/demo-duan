using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int Duration { get; set; }
        
        [StringLength(200)]
        public string? Img { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public DateTime ReleaseDate { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        // Foreign Key
        public int? CategoryId { get; set; }
        
        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        
        // Method để cập nhật Status dựa trên ReleaseDate
        public void UpdateStatusBasedOnReleaseDate()
        {
            Status = ReleaseDate <= DateTime.Now ? "Đang chiếu" : "Sắp chiếu";
        }
        
        // Computed property để hiển thị Status tự động
        [NotMapped]
        public string ComputedStatus => ReleaseDate <= DateTime.Now ? "Đang chiếu" : "Sắp chiếu";
    }
}
