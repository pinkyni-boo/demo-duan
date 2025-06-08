using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new DashboardStats
            {
                TotalMovies = await _context.Movies.CountAsync(),
                TotalTheaters = await _context.Theaters.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalShowtimes = await _context.Showtimes.CountAsync(),
                TotalTickets = await _context.Tickets.CountAsync(),
                TodayShowtimesCount = await _context.Showtimes
                    .Where(s => s.Date.Date == DateTime.Today)
                    .CountAsync(),
                ComingSoonMovies = await _context.Movies
                    .Where(m => m.ReleaseDate > DateTime.Now)
                    .CountAsync(),
                ActiveMovies = await _context.Movies
                    .Where(m => m.ReleaseDate <= DateTime.Now)
                    .CountAsync(),
                RecentMovies = await _context.Movies
                    .Include(m => m.Category)
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToListAsync(),
                UpcomingShowtimes = await _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Theater)
                    .Where(s => s.Date >= DateTime.Today)
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.Time)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }
    }

    public class DashboardStats
    {
        public int TotalMovies { get; set; }
        public int TotalTheaters { get; set; }
        public int TotalCategories { get; set; }
        public int TotalShowtimes { get; set; }
        public int TotalTickets { get; set; }
        public int TodayShowtimesCount { get; set; }
        public int ComingSoonMovies { get; set; }
        public int ActiveMovies { get; set; }
        public List<demo_duan.Models.Movie> RecentMovies { get; set; } = new();
        public List<demo_duan.Models.Showtime> UpcomingShowtimes { get; set; } = new();
    }
}