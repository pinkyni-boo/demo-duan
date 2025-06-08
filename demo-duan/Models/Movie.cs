using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public enum MovieStatus
    {
        ComingSoon,
        NowShowing,
        Ended,
        NotShowing // Add missing status
    }

    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Duration { get; set; } // In minutes

        [Required]
        public decimal Rating { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(255)]
        public string? Img { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? Language { get; set; }

        [StringLength(100)]
        public string? Director { get; set; }

        [StringLength(255)]
        public string? Cast { get; set; }

        [Required]
        public MovieStatus Status { get; set; } = MovieStatus.ComingSoon;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Helper methods
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    MovieStatus.ComingSoon => "Sắp chiếu",
                    MovieStatus.NowShowing => "Đang chiếu",
                    MovieStatus.Ended => "Đã kết thúc",
                    MovieStatus.NotShowing => "Ngừng chiếu",
                    _ => "Không xác định"
                };
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    MovieStatus.ComingSoon => "bg-info",
                    MovieStatus.NowShowing => "bg-success",
                    MovieStatus.Ended => "bg-secondary",
                    MovieStatus.NotShowing => "bg-danger",
                    _ => "bg-warning"
                };
            }
        }

        public void UpdateStatusBasedOnReleaseDate()
        {
            var today = DateTime.Today;
            
            // Coming Soon: Release date is in the future
            if (ReleaseDate > today.AddDays(1))
            {
                Status = MovieStatus.ComingSoon;
            }
            // Now Showing: Release date is within the last 60 days
            else if (ReleaseDate <= today && ReleaseDate >= today.AddDays(-60))
            {
                Status = MovieStatus.NowShowing;
            }
            // Ended: Release date was more than 60 days ago
            else
            {
                Status = MovieStatus.Ended;
            }
        }
    }
}
