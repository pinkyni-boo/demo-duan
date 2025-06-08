using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace demo_duan.Models
{
    public class Theater
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên rạp")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Thành phố")]
        public string City { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string? Image { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Cinema> Cinemas { get; set; } = new List<Cinema>();

        // Thêm computed property cho Showtimes
        [NotMapped]
        public virtual ICollection<Showtime> Showtimes => 
            Cinemas?.SelectMany(c => c.Showtimes).ToList() ?? new List<Showtime>();

        // Thêm mutable TotalCapacity để CinemasController có thể update
        [NotMapped]
        [Display(Name = "Tổng số ghế")]
        public int TotalCapacity { get; set; }
    }
}