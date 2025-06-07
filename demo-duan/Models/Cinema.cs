using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên phòng chiếu là bắt buộc")]
        [Display(Name = "Tên phòng chiếu")]
        [StringLength(50, ErrorMessage = "Tên phòng chiếu không được quá 50 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn rạp")]
        [Display(Name = "Rạp chiếu phim")]
        public int TheaterId { get; set; }

        [Required(ErrorMessage = "Số ghế là bắt buộc")]
        [Display(Name = "Số ghế")]
        [Range(10, 500, ErrorMessage = "Số ghế phải từ 10 đến 500")]
        public int Seats { get; set; }

        [Display(Name = "Loại phòng")]
        [StringLength(20)]
        public string Type { get; set; } = "Standard"; // Standard, VIP, IMAX, 4DX

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual Theater Theater { get; set; } = null!;
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}