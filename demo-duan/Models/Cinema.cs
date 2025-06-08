using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên phòng chiếu")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Rạp")]
        public int TheaterId { get; set; }

        [Required]
        [Display(Name = "Tổng số ghế")]
        public int TotalSeats { get; set; } = 0;

        [Required]
        [Display(Name = "Số hàng")]
        public int Rows { get; set; } = 10;

        [Required]
        [Display(Name = "Số ghế mỗi hàng")]
        public int SeatsPerRow { get; set; } = 10;

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại phòng")]
        public string Type { get; set; } = "2D"; // 2D, 3D, 4DX, IMAX, etc.

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Computed property for seats (backward compatibility)
        [NotMapped]
        [Display(Name = "Số ghế")]
        public int Seats => Rows * SeatsPerRow;

        // Navigation properties
        public virtual Theater Theater { get; set; } = null!;
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}