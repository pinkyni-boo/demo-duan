using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public enum PaymentStatus
    {
        [Display(Name = "Chờ thanh toán")]
        Pending,
        [Display(Name = "Đã thanh toán")]
        Completed,
        [Display(Name = "Thất bại")]
        Failed,
        [Display(Name = "Đã hủy")]
        Cancelled,
        [Display(Name = "Đã hoàn tiền")]
        Refunded
    }

    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Display(Name = "Trạng thái")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Display(Name = "Mã giao dịch")]
        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Ngày xử lý")]
        public DateTime? ProcessedDate { get; set; }

        // Navigation Properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
        public virtual IdentityUser User { get; set; } = null!;

        [NotMapped]
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    PaymentStatus.Pending => "Chờ thanh toán",
                    PaymentStatus.Completed => "Đã thanh toán",
                    PaymentStatus.Failed => "Thất bại",
                    PaymentStatus.Cancelled => "Đã hủy",
                    PaymentStatus.Refunded => "Đã hoàn tiền",
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
                    PaymentStatus.Pending => "bg-warning text-dark",
                    PaymentStatus.Completed => "bg-success",
                    PaymentStatus.Failed => "bg-danger",
                    PaymentStatus.Cancelled => "bg-secondary",
                    PaymentStatus.Refunded => "bg-info",
                    _ => "bg-secondary"
                };
            }
        }

        [NotMapped]
        public string StatusIcon
        {
            get
            {
                return Status switch
                {
                    PaymentStatus.Pending => "fas fa-clock",
                    PaymentStatus.Completed => "fas fa-check-circle",
                    PaymentStatus.Failed => "fas fa-times-circle",
                    PaymentStatus.Cancelled => "fas fa-ban",
                    PaymentStatus.Refunded => "fas fa-undo",
                    _ => "fas fa-question-circle"
                };
            }
        }
    }
}