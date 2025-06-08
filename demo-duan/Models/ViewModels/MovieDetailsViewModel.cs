namespace demo_duan.Models
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; } = null!;
        public List<Movie> RelatedMovies { get; set; } = new();
        public List<Showtime> AvailableShowtimes { get; set; } = new();
    }
}