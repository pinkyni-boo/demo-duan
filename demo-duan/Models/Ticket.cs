namespace demo_duan.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int ShowtimeId { get; set; }
        public string CustomerName { get; set; }
        public int Seats { get; set; }

        public virtual Showtime Showtime { get; set; }
    }

}
