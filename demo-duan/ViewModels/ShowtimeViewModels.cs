namespace demo_duan.ViewModels
{
    public class ShowtimeViewModel
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
        public string CinemaType { get; set; }
        public List<ShowtimeItemViewModel> Showtimes { get; set; } = new List<ShowtimeItemViewModel>();
    }

    public class ShowtimeItemViewModel
    {
        public int Id { get; set; }
        public string ShowTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
    }
}