using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Showtimes
        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();
            return View(showtimes);
        }

        // GET: Showtimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // GET: Showtimes/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: Showtimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,TheaterId,Date,Time,AvailableSeats")] Showtime showtime)
        {
            try
            {
                Console.WriteLine($"Creating showtime - MovieId: {showtime.MovieId}, TheaterId: {showtime.TheaterId}");
                Console.WriteLine($"Date: {showtime.Date}, Time: {showtime.Time}");

                if (ModelState.IsValid)
                {
                    // Kiểm tra conflict về thời gian
                    var conflict = await _context.Showtimes
                        .AnyAsync(s => s.TheaterId == showtime.TheaterId && 
                                      s.Date.Date == showtime.Date.Date && 
                                      s.Time == showtime.Time);

                    if (conflict)
                    {
                        ModelState.AddModelError("", "This theater already has a showtime at the selected date and time.");
                    }
                    else
                    {
                        // Nếu AvailableSeats không được set, lấy từ Theater capacity
                        if (showtime.AvailableSeats <= 0)
                        {
                            var theater = await _context.Theaters.FindAsync(showtime.TheaterId);
                            showtime.AvailableSeats = theater?.Capacity ?? 100;
                        }

                        _context.Add(showtime);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Showtime created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating showtime: {ex.Message}");
                TempData["Error"] = "An error occurred while creating the showtime.";
            }

            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // GET: Showtimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }

            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // POST: Showtimes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,TheaterId,Date,Time,AvailableSeats")] Showtime showtime)
        {
            if (id != showtime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra conflict (ngoại trừ chính nó)
                    var conflict = await _context.Showtimes
                        .AnyAsync(s => s.Id != showtime.Id &&
                                      s.TheaterId == showtime.TheaterId && 
                                      s.Date.Date == showtime.Date.Date && 
                                      s.Time == showtime.Time);

                    if (conflict)
                    {
                        ModelState.AddModelError("", "This theater already has a showtime at the selected date and time.");
                    }
                    else
                    {
                        _context.Update(showtime);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Showtime updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(showtime.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await LoadDropdownData(showtime);
            return View(showtime);
        }

        // GET: Showtimes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: Showtimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime != null)
            {
                if (showtime.Tickets.Any())
                {
                    TempData["Error"] = "Cannot delete showtime because tickets have been booked for this showtime.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Showtime deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.Id == id);
        }

        private async Task LoadDropdownData(Showtime? showtime = null)
        {
            ViewData["MovieId"] = new SelectList(
                await _context.Movies.ToListAsync(), 
                "Id", "Title", showtime?.MovieId);
            
            ViewData["TheaterId"] = new SelectList(
                await _context.Theaters.ToListAsync(), 
                "Id", "Name", showtime?.TheaterId);
        }

        // Seed sample showtimes
        public async Task<IActionResult> SeedData()
        {
            try
            {
                if (!await _context.Showtimes.AnyAsync())
                {
                    var movies = await _context.Movies.ToListAsync();
                    var theaters = await _context.Theaters.ToListAsync();

                    if (!movies.Any() || !theaters.Any())
                    {
                        return Json(new { success = false, message = "Please create movies and theaters first" });
                    }

                    var showtimes = new List<Showtime>();
                    var today = DateTime.Today;

                    // Tạo showtimes cho 7 ngày tới
                    for (int day = 0; day < 7; day++)
                    {
                        var date = today.AddDays(day);
                        
                        foreach (var theater in theaters)
                        {
                            // Mỗi rạp có 3 suất chiếu mỗi ngày
                            var times = new TimeSpan[] { 
                                new TimeSpan(14, 0, 0), // 2:00 PM
                                new TimeSpan(17, 30, 0), // 5:30 PM
                                new TimeSpan(20, 0, 0)   // 8:00 PM
                            };

                            for (int i = 0; i < times.Length && i < movies.Count; i++)
                            {
                                showtimes.Add(new Showtime
                                {
                                    MovieId = movies[i % movies.Count].Id,
                                    TheaterId = theater.Id,
                                    Date = date,
                                    Time = times[i],
                                    AvailableSeats = theater.Capacity
                                });
                            }
                        }
                    }

                    _context.Showtimes.AddRange(showtimes);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = $"Created {showtimes.Count} sample showtimes" });
                }

                return Json(new { success = true, message = "Showtimes already exist" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}