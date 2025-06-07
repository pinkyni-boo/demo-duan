using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Showtimes
        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Cinema)
                .Include(s => s.Tickets)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();
            return View(showtimes);
        }

        // GET: Admin/Showtimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Cinema)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null) return NotFound();

            return View(showtime);
        }

        // GET: Admin/Showtimes/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var movies = await _context.Movies.Where(m => m.ReleaseDate <= DateTime.Today.AddDays(90)).ToListAsync();
                var theaters = await _context.Theaters.Where(t => t.IsActive).ToListAsync();

                if (!movies.Any())
                {
                    TempData["ErrorMessage"] = "Chưa có phim nào. Vui lòng thêm phim trước.";
                    return RedirectToAction("Index", "Movies");
                }

                if (!theaters.Any())
                {
                    TempData["ErrorMessage"] = "Chưa có rạp nào. Vui lòng thêm rạp trước.";
                    return RedirectToAction("Index", "Theaters");
                }

                ViewBag.MovieId = new SelectList(movies, "Id", "Title");
                ViewBag.TheaterId = new SelectList(theaters, "Id", "Name");
                ViewBag.CinemaId = new SelectList(new List<Cinema>(), "Id", "Name");
                
                var showtime = new Showtime
                {
                    Date = DateTime.Today.AddDays(1),
                    Time = new TimeSpan(19, 0, 0),
                    Status = "Scheduled"
                };
                
                return View(showtime);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải trang: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Showtimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showtime showtime)
        {
            try
            {
                // Remove validation for navigation properties
                ModelState.Remove("Movie");
                ModelState.Remove("Theater");
                ModelState.Remove("Cinema");
                ModelState.Remove("Tickets");

                // Custom validation
                if (showtime.MovieId <= 0)
                    ModelState.AddModelError("MovieId", "Vui lòng chọn phim.");

                if (showtime.TheaterId <= 0)
                    ModelState.AddModelError("TheaterId", "Vui lòng chọn rạp chiếu.");

                if (showtime.CinemaId <= 0)
                    ModelState.AddModelError("CinemaId", "Vui lòng chọn phòng chiếu.");

                if (showtime.Date < DateTime.Today)
                    ModelState.AddModelError("Date", "Ngày chiếu không thể là ngày trong quá khứ.");

                // Check for conflicting showtimes in the same cinema
                var conflictingShowtime = await _context.Showtimes
                    .Where(s => s.CinemaId == showtime.CinemaId && 
                               s.Date.Date == showtime.Date.Date && 
                               s.Time == showtime.Time &&
                               s.Status != "Cancelled")
                    .FirstOrDefaultAsync();

                if (conflictingShowtime != null)
                    ModelState.AddModelError("", "Đã có suất chiếu khác tại phòng này vào thời gian này.");

                // Validate cinema belongs to theater
                var cinema = await _context.Cinemas
                    .Where(c => c.Id == showtime.CinemaId && c.TheaterId == showtime.TheaterId)
                    .FirstOrDefaultAsync();

                if (cinema == null)
                    ModelState.AddModelError("CinemaId", "Phòng chiếu không thuộc rạp đã chọn.");

                if (ModelState.IsValid && cinema != null)
                {
                    // Set seats and price from cinema and movie
                    showtime.TotalSeats = cinema.Seats;
                    showtime.AvailableSeats = cinema.Seats;
                    
                    // Get movie price
                    var movie = await _context.Movies.FindAsync(showtime.MovieId);
                    showtime.Price = movie?.Price ?? 0;

                    _context.Add(showtime);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Suất chiếu đã được tạo thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo suất chiếu: " + ex.Message);
            }

            // Reload dropdowns for the view
            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // Thêm method này vào ShowtimesController

        [HttpGet]
        public async Task<IActionResult> GetCinemasByTheater(int theaterId)
        {
            try
            {
                var cinemas = await _context.Cinemas
                    .Where(c => c.TheaterId == theaterId && c.IsActive)
                    .Select(c => new { 
                        id = c.Id, 
                        name = c.Name, 
                        seats = c.Seats,
                        type = c.Type ?? "Standard"
                    })
                    .OrderBy(c => c.name)
                    .ToListAsync();

                return Json(cinemas);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi khi tải phòng chiếu: " + ex.Message });
            }
        }

        private async Task LoadDropdownData(Showtime? showtime = null)
        {
            // Sửa điều kiện lọc Movie
            var movies = await _context.Movies
                .Where(m => m.IsActive == true) // Sửa thành == true thay vì chỉ m.IsActive
                .OrderBy(m => m.Title)
                .ToListAsync();

            var theaters = await _context.Theaters
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.MovieId = new SelectList(movies, "Id", "Title", showtime?.MovieId);
            ViewBag.TheaterId = new SelectList(theaters, "Id", "Name");
            
            // Load cinemas if showtime has cinema selected
            if (showtime?.CinemaId != null)
            {
                var cinema = await _context.Cinemas
                    .Include(c => c.Theater)
                    .FirstOrDefaultAsync(c => c.Id == showtime.CinemaId);
                    
                if (cinema != null)
                {
                    var cinemas = await _context.Cinemas
                        .Where(c => c.TheaterId == cinema.TheaterId && c.IsActive)
                        .ToListAsync();
                        
                    ViewBag.CinemaId = new SelectList(cinemas, "Id", "Name", showtime.CinemaId);
                    ViewBag.SelectedTheaterId = cinema.TheaterId;
                }
            }
        }
    }
}