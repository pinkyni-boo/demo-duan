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

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: Admin/Showtimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showtime model)
        {
            // Thêm debug logging
            System.Diagnostics.Debug.WriteLine($"Received data: MovieId={model.MovieId}, CinemaId={model.CinemaId}");
            
            // Fix lỗi bằng cách kiểm tra thủ công
            if (model.MovieId == 0)
            {
                ModelState.AddModelError("MovieId", "Phim không được để trống");
            }
            
            if (model.CinemaId == 0)
            {
                ModelState.AddModelError("CinemaId", "Phòng chiếu không được để trống");
            }
            
            // Nếu model valid (hoặc bỏ qua validation khi debug)
            if (ModelState.IsValid)
            {
                try
                {
                    // Đảm bảo các trường khác đã được set
                    model.Status = "Available";
                    model.IsActive = true;
                    model.CreatedDate = DateTime.Now;
                    
                    // Đảm bảo ShowDate không phải là default
                    if (model.ShowDate == default)
                    {
                        model.ShowDate = DateTime.Today;
                    }
                    
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    System.Diagnostics.Debug.WriteLine($"Error saving showtime: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu. Chi tiết: " + ex.Message);
                }
            }
            
            // Log tất cả lỗi validation
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Validation error for {state.Key}: {error.ErrorMessage}");
                }
            }
            
            // Tạo lại danh sách cho dropdown
            ViewBag.MovieId = new SelectList(_context.Movies, "Id", "Title", model.MovieId);
            ViewBag.TheaterId = new SelectList(_context.Theaters, "Id", "Name");
            return View(model);
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
        public async Task<IActionResult> GetCinemasByTheater(int theaterId)
        {
            var cinemas = await _context.Cinemas
                .Where(c => c.TheaterId == theaterId)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    seats = c.TotalSeats,
                    type = c.Type // Thêm trường type
                })
                .ToListAsync();

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