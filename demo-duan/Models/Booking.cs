namespace demo_duan.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ShowtimeId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }

        public Showtime? Showtime { get; set; }
    }
}
