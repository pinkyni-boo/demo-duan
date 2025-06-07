using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public enum MovieStatus
    {
        [Display(Name = "Sắp chiếu")]
        ComingSoon,
        [Display(Name = "Đang chiếu")]
        NowShowing,
        [Display(Name = "Ngừng chiếu")]
        NotShowing,
        [Display(Name = "Tạm ngưng")]
        Suspended
    }

    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề phim là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Display(Name = "Mô tả")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời lượng là bắt buộc")]
        [Display(Name = "Thời lượng (phút)")]
        [Range(1, 500, ErrorMessage = "Thời lượng phải từ 1 đến 500 phút")]
        public int Duration { get; set; }

        [Display(Name = "Ngày phát hành")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Giá vé là bắt buộc")]
        [Display(Name = "Giá vé")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        [StringLength(255)]
        public string? Img { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Trạng thái chiếu")]
        public MovieStatus Status { get; set; } = MovieStatus.ComingSoon;

        [Display(Name = "Đánh giá")]
        [Range(0, 10, ErrorMessage = "Đánh giá từ 0 đến 10")]
        public decimal Rating { get; set; } = 0;

        [Display(Name = "Ngôn ngữ")]
        [StringLength(50)]
        public string? Language { get; set; } = "Tiếng Việt";

        [Display(Name = "Đạo diễn")]
        [StringLength(200)]
        public string? Director { get; set; }

        [Display(Name = "Diễn viên")]
        [StringLength(500)]
        public string? Cast { get; set; }

        [Display(Name = "Trailer URL")]
        [StringLength(255)]
        public string? TrailerUrl { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Methods
        public void UpdateStatusBasedOnReleaseDate()
        {
            var today = DateTime.Today;
            var releaseDate = ReleaseDate.Date;

            if (!IsActive)
            {
                Status = MovieStatus.Suspended;
            }
            else if (releaseDate > today)
            {
                Status = MovieStatus.ComingSoon;
            }
            else if (releaseDate <= today)
            {
                Status = MovieStatus.NowShowing;
            }
            else
            {
                Status = MovieStatus.NotShowing;
            }
        }

        [NotMapped]
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    MovieStatus.ComingSoon => "Sắp chiếu",
                    MovieStatus.NowShowing => "Đang chiếu",
                    MovieStatus.NotShowing => "Ngừng chiếu",
                    MovieStatus.Suspended => "Tạm ngưng",
                    _ => "Không xác định"
                };
            }
        }

        [NotMapped]
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    MovieStatus.ComingSoon => "bg-info",
                    MovieStatus.NowShowing => "bg-success",
                    MovieStatus.NotShowing => "bg-secondary",
                    MovieStatus.Suspended => "bg-warning",
                    _ => "bg-secondary"
                };
            }
        }
    }
}
