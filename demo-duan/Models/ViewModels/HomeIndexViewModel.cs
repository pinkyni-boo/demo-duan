
namespace demo_duan.Models
{
    public class HomeIndexViewModel
    {
        public List<Movie> FeaturedMovies { get; set; } = new();
        public List<Movie> ComingSoonMovies { get; set; } = new();
        public List<Theater> Theaters { get; set; } = new();
        public int TotalMovies { get; set; }
        public int TotalTheaters { get; set; }
        public int TotalUsers { get; set; }
    }
}