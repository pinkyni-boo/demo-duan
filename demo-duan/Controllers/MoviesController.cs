using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies - Trang chính hiển thị phim
        public async Task<IActionResult> Index(int? categoryId, string searchTerm)
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

            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Movies/NowShowing - Phim đang chiếu
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
            return View("Index", movies);
        }

        // GET: Movies/ComingSoon - Phim sắp chiếu
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
            return View("Index", movies);
        }

        // GET: Movies/BookTicket/5 - Chọn rạp và suất chiếu
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
                return RedirectToAction("Details", new { id = id });
            }

            return View(movie);
        }

        // GET: Movies/SelectSeats/5 - Chọn ghế
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

            // Kiểm tra thời gian
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

            var bookedSeats = showtime.Tickets.Select(t => t.Seat).ToList();
            ViewBag.BookedSeats = bookedSeats;

            return View(showtime);
        }
    }
}
