using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phim")]
        [Display(Name = "Phim")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn rạp chiếu")]
        [Display(Name = "Rạp chiếu phim")]
        public int TheaterId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng chiếu")]
        [Display(Name = "Phòng chiếu")]
        public int CinemaId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày chiếu")]
        [Display(Name = "Ngày chiếu")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ chiếu")]
        [Display(Name = "Giờ chiếu")]
        public TimeSpan Time { get; set; }

        [Display(Name = "Số ghế có sẵn")]
        [Range(0, 1000, ErrorMessage = "Số ghế phải từ 0 đến 1000")]
        public int AvailableSeats { get; set; }

        [Display(Name = "Tổng số ghế")]
        public int TotalSeats { get; set; }

        [Display(Name = "Giá vé")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Cancelled, Completed

        // Navigation properties
        public virtual Movie Movie { get; set; } = null!;
        public virtual Theater Theater { get; set; } = null!;
        public virtual Cinema Cinema { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
