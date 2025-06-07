using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace demo_duan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Home - Trang chủ
        public async Task<IActionResult> Index()
        {
            try
            {
                var movies = await _context.Movies
                    .Include(m => m.Category)
                    .Where(m => m.IsActive && m.Status == MovieStatus.NowShowing)
                    .OrderByDescending(m => m.ReleaseDate)
                    .Take(12)
                    .ToListAsync();

                var categories = await _context.Categories
                    .Where(c => c.Movies.Any(m => m.IsActive))
                    .ToListAsync();

                // Sửa dòng 84 - kiểm tra null an toàn
                var featuredMovies = movies
                    .Where(m => m.Category != null && m.Category.Name != null && m.Category.Name.Equals("Hành động", StringComparison.OrdinalIgnoreCase))
                    .Take(6)
                    .ToList();

                ViewBag.Categories = categories;
                ViewBag.FeaturedMovies = featuredMovies;

                return View(movies);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu trang chủ: " + ex.Message;
                return View(new List<Movie>());
            }
        }

        // GET: Home/Movies - Tất cả phim cho người dùng
        public async Task<IActionResult> Movies(int? categoryId, string searchTerm)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = searchTerm;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.Date >= DateTime.Today))
                .ThenInclude(s => s.Theater)
                .AsQueryable();

            // Lọc theo category
            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchTerm))
            {
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchTerm) || 
                                                    m.Description.Contains(searchTerm));
            }

            var movies = await moviesQuery
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "All Movies";
            return View(movies);
        }

        // GET: Home/NowShowing - Phim đang chiếu
        public async Task<IActionResult> NowShowing(int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.Date >= DateTime.Today))
                .ThenInclude(s => s.Theater)
                .Where(m => m.Showtimes.Any(s => s.Date >= DateTime.Today))
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            var movies = await moviesQuery
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "Now Showing";
            return View("Movies", movies);
        }

        // GET: Home/ComingSoon - Phim sắp chiếu
        public async Task<IActionResult> ComingSoon(int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .ThenInclude(s => s.Theater)
                .Where(m => m.ReleaseDate > DateTime.Today || 
                           !m.Showtimes.Any(s => s.Date >= DateTime.Today))
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            var movies = await moviesQuery
                .OrderBy(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "Coming Soon";
            return View("Movies", movies);
        }

        // GET: Home/MovieDetails/5
        public async Task<IActionResult> MovieDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.Date >= DateTime.Today))
                .ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Home/BookTicket/5 - Yêu cầu đăng nhập
        [Authorize]
        public async Task<IActionResult> BookTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.Date >= DateTime.Today && s.AvailableSeats > 0))
                .ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            if (!movie.Showtimes.Any())
            {
                TempData["Error"] = "No available showtimes for this movie.";
                return RedirectToAction("MovieDetails", new { id = id });
            }

            return View(movie);
        }

        // GET: Home/SelectSeats/5 - Yêu cầu đăng nhập
        [Authorize]
        public async Task<IActionResult> SelectSeats(int? showtimeId)
        {
            if (showtimeId == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .ThenInclude(m => m.Category)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            var showtimeDateTime = showtime.Date.Date.Add(showtime.Time);
            if (showtimeDateTime < DateTime.Now)
            {
                TempData["Error"] = "Cannot book tickets for past showtimes.";
                return RedirectToAction("BookTicket", new { id = showtime.MovieId });
            }

            if (showtime.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats for this showtime.";
                return RedirectToAction("BookTicket", new { id = showtime.MovieId });
            }

            var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
            ViewBag.BookedSeats = bookedSeats;

            return View(showtime);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}