namespace demo_duan.Models
{
    public class ShowtimesIndexViewModel
    {
        public List<Showtime> Showtimes { get; set; } = new();
        public List<Theater> Theaters { get; set; } = new();
        public List<Movie> Movies { get; set; } = new();
        public int? SelectedTheaterId { get; set; }
        public int? SelectedMovieId { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
    }
}