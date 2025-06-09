using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;
using System.Globalization;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShowtimesController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShowtimesController> _logger;

        public ShowtimesController(ApplicationDbContext context, ILogger<ShowtimesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Showtimes
        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .OrderByDescending(s => s.ShowDate)
                .ThenBy(s => s.ShowTime)
                .ToListAsync();
            return View(showtimes);
        }

        // GET: Admin/Showtimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .Include(s => s.Tickets)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null) return NotFound();

            return View(showtime);
        }

        // GET: Admin/Showtimes/Create
        public IActionResult Create()
        {
            // Populate ViewBag with data for dropdowns
            ViewBag.Movies = _context.Movies.ToList();
            ViewBag.Theaters = _context.Theaters.ToList();
            return View();
        }

        // POST: Admin/Showtimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection formCollection)
        {
            // Tạo mới đối tượng Showtime từ form data
            var showtime = new Showtime();
            
            // Gán giá trị từ form một cách rõ ràng
            if (int.TryParse(formCollection["MovieId"], out int movieId))
            {
                showtime.MovieId = movieId;
            }
            
            if (int.TryParse(formCollection["CinemaId"], out int cinemaId))
            {
                showtime.CinemaId = cinemaId;
            }
            
            if (DateTime.TryParse(formCollection["ShowDate"], out DateTime showDate))
            {
                showtime.ShowDate = showDate;
            }
            
            if (TimeSpan.TryParse(formCollection["ShowTime"], out TimeSpan showTime))
            {
                showtime.ShowTime = showTime;
            }
            
            if (decimal.TryParse(formCollection["Price"], out decimal price))
            {
                showtime.Price = price;
            }
            
            // Kiểm tra dữ liệu thủ công
            bool isValid = true;
            
            if (showtime.MovieId <= 0)
            {
                ModelState.AddModelError("MovieId", "The Movie field is required.");
                isValid = false;
            }
            
            if (showtime.CinemaId <= 0)
            {
                ModelState.AddModelError("CinemaId", "The Cinema field is required.");
                isValid = false;
            }
            
            if (isValid)
            {
                try
                {
                    // Thiết lập các giá trị mặc định
                    showtime.Status = "Available";
                    showtime.IsActive = true;
                    showtime.CreatedDate = DateTime.Now;
                    
                    // Thêm và lưu
                    _context.Add(showtime);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    ModelState.AddModelError("", "Lỗi khi lưu: " + ex.Message);
                }
            }
            
            // Nếu không thành công, hiển thị form lại
            ViewBag.Movies = _context.Movies.ToList();
            ViewBag.Theaters = _context.Theaters.ToList();
            
            return View(showtime);
        }

        // GET: Admin/Showtimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null) return NotFound();

            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // POST: Admin/Showtimes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,CinemaId,ShowDate,ShowTime,Price,Status,IsActive")] Showtime showtime)
        {
            if (id != showtime.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showtime);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lịch chiếu đã được cập nhật!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(showtime.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                }
            }

            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // GET: Admin/Showtimes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null) return NotFound();

            return View(showtime);
        }

        // POST: Admin/Showtimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lịch chiếu đã được xóa!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: Get cinemas by theater
        [HttpGet]
        public JsonResult GetCinemasByTheater(int theaterId)
        {
            var cinemas = _context.Cinemas
                .Where(c => c.TheaterId == theaterId)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    seats = c.TotalSeats,
                    type = c.Type
                })
                .ToList();

            // Log số lượng phòng chiếu tìm thấy
            System.Diagnostics.Debug.WriteLine($"Found {cinemas.Count} cinemas for theater {theaterId}");
            
            // Log chi tiết từng phòng
            foreach (var cinema in cinemas)
            {
                System.Diagnostics.Debug.WriteLine($"Cinema: Id={cinema.id}, Name={cinema.name}");
            }

            return Json(cinemas);
        }

        // API: Check conflict showtimes
        [HttpGet]
        public async Task<IActionResult> CheckConflict(int cinemaId, string date, string time, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
                return Json(new { conflict = false });

            var checkDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var checkTime = TimeSpan.Parse(time);

            // Check +/- 2 hours from the requested time
            var startTime = checkTime.Add(TimeSpan.FromHours(-2));
            var endTime = checkTime.Add(TimeSpan.FromHours(2));

            var conflict = await _context.Showtimes
                .Where(s => s.CinemaId == cinemaId &&
                            s.ShowDate.Date == checkDate.Date &&
                            s.ShowTime >= startTime &&
                            s.ShowTime <= endTime &&
                            (excludeId == null || s.Id != excludeId))
                .Select(s => new { s.Id, MovieTitle = s.Movie.Title, s.ShowTime })
                .ToListAsync();

            return Json(new { conflict = conflict.Any(), showtimes = conflict });
        }

        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.Id == id);
        }

        private async Task LoadDropdownData(Showtime? showtime = null)
        {
            // Get Theaters
            var theaters = await _context.Theaters
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            // Get Movies
            var movies = await _context.Movies
                .Where(m => m.IsActive && (m.Status == MovieStatus.NowShowing || m.Status == MovieStatus.ComingSoon))
                .OrderBy(m => m.Title)
                .ToListAsync();

            // Get Cinemas based on selected theater or first theater
            var theaterId = showtime?.Cinema?.TheaterId ?? (theaters.Any() ? theaters.First().Id : 0);
            var cinemas = await _context.Cinemas
                .Where(c => c.TheaterId == theaterId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.TheaterId = new SelectList(theaters, "Id", "Name", theaterId);
            ViewBag.MovieId = new SelectList(movies, "Id", "Title", showtime?.MovieId);
            ViewBag.CinemaId = new SelectList(cinemas, "Id", "Name", showtime?.CinemaId);
        }
    }
}