using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên phương thức")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Icon/Logo")]
        [StringLength(255)]
        public string? Icon { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Phí giao dịch (%)")]
        [Range(0, 100)]
        public decimal TransactionFee { get; set; } = 0;

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        // Navigation Properties
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}