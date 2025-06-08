using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_duan.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int CinemaId { get; set; }

        // Thêm TheaterId navigation
        [NotMapped]
        public int TheaterId => Cinema?.TheaterId ?? 0;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày chiếu")]
        public DateTime ShowDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ chiếu")]
        public TimeSpan ShowTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá vé")]
        public decimal Price { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Available"; // Available, Full, Cancelled, Completed

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Movie Movie { get; set; } = null!;
        public virtual Cinema Cinema { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Computed properties
        [NotMapped]
        [Display(Name = "Ngày")]
        public DateTime Date => ShowDate;

        [NotMapped]
        [Display(Name = "Giờ")]
        public TimeSpan Time => ShowTime;

        [NotMapped]
        [Display(Name = "Rạp chiếu")]
        public virtual Theater? Theater => Cinema?.Theater;

        [NotMapped]
        [Display(Name = "Tổng số ghế")]
        public int TotalSeats => Cinema?.TotalSeats ?? 0;

        [NotMapped]
        [Display(Name = "Số ghế đã đặt")]
        public int BookedSeats => Tickets?.Count(t => t.Status != "Cancelled") ?? 0;

        [NotMapped]
        [Display(Name = "Số ghế còn trống")]
        public int AvailableSeats => TotalSeats - BookedSeats;
    }
}
