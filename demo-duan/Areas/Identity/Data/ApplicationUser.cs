using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using demo_duan.Models;

namespace demo_duan.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(500)]
        public string? Address { get; set; }
    [PersonalData]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [StringLength(255)]
        public string? Avatar { get; set; }

        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Lần đăng nhập cuối")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Trạng thái tài khoản")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation Properties - Fixed namespace references
        public virtual ICollection<demo_duan.Models.Ticket> Tickets { get; set; } = new List<demo_duan.Models.Ticket>();
        public virtual ICollection<demo_duan.Models.Payment> Payments { get; set; } = new List<demo_duan.Models.Payment>();
    }
}