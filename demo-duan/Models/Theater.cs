using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace demo_duan.Models
{
    public class Theater
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên rạp là bắt buộc")]
        [Display(Name = "Tên rạp")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        [StringLength(200)]
        public string Address { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Thành phố")]
        [StringLength(50)]
        public string? City { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Tổng số ghế")]
        public int TotalCapacity { get; set; } = 0; // Thêm setter

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Cinema> Cinemas { get; set; } = new List<Cinema>();
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}