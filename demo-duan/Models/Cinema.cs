using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace demo_duan.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên phòng chiếu không được để trống")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn rạp chiếu phim")]
        [Display(Name = "Rạp chiếu phim")]
        public int TheaterId { get; set; }
        
        [Required(ErrorMessage = "Số hàng không được để trống")]
        [Range(1, 50, ErrorMessage = "Số hàng phải từ 1-50")]
        [Display(Name = "Số hàng")]
        public int Rows { get; set; } = 10;

        [Required(ErrorMessage = "Số ghế mỗi hàng không được để trống")]
        [Range(1, 50, ErrorMessage = "Số ghế mỗi hàng phải từ 1-50")]
        [Display(Name = "Số ghế mỗi hàng")]
        public int SeatsPerRow { get; set; } = 10;

        // QUAN TRỌNG: KHÔNG SỬ DỤNG [NotMapped] ở đây
        [Display(Name = "Số ghế")]
        public int TotalSeats { get; set; } // Loại bỏ computed property và NotMapped

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

        // Navigation properties
        [ValidateNever] // Thêm thuộc tính này để tránh validation
        public virtual Theater? Theater { get; set; } // Đổi thành nullable
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}