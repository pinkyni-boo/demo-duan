using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemasController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public CinemasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Cinemas
        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas
                .Include(c => c.Theater)
                .OrderBy(c => c.Theater.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(cinemas);
        }

        // GET: Admin/Cinemas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cinema = await _context.Cinemas
                .Include(c => c.Theater)
                .Include(c => c.Showtimes)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null) return NotFound();

            return View(cinema);
        }

        // GET: Admin/Cinemas/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: Admin/Cinemas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TheaterId,Name,TotalSeats,Type,Description,IsActive,Rows,SeatsPerRow")] Cinema cinema)
        {
            ModelState.Remove("Theater");
            ModelState.Remove("Showtimes");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(cinema);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Phòng chiếu đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tạo phòng chiếu: " + ex.Message;
                }
            }

            await LoadDropdownData(cinema);
            return View(cinema);
        }

        // GET: Admin/Cinemas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null) return NotFound();

            await LoadDropdownData(cinema);
            return View(cinema);
        }

        // POST: Admin/Cinemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TheaterId,Name,TotalSeats,Type,Description,IsActive,Rows,SeatsPerRow")] Cinema cinema)
        {
            if (id != cinema.Id) return NotFound();

            ModelState.Remove("Theater");
            ModelState.Remove("Showtimes");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldTheaterId = await _context.Cinemas
                        .Where(c => c.Id == id)
                        .Select(c => c.TheaterId)
                        .FirstAsync();

                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Phòng chiếu đã được cập nhật!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CinemaExists(cinema.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                }
            }

            await LoadDropdownData(cinema);
            return View(cinema);
        }

        // GET: Admin/Cinemas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cinema = await _context.Cinemas
                .Include(c => c.Theater)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null) return NotFound();

            return View(cinema);
        }

        // POST: Admin/Cinemas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema != null)
            {
                var theaterId = cinema.TheaterId;
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Phòng chiếu đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Get cinemas by theater
        [HttpGet]
        public async Task<IActionResult> GetCinemasByTheater(int theaterId)
        {
            var cinemas = await _context.Cinemas
                .Where(c => c.TheaterId == theaterId && c.IsActive)
                .Select(c => new { c.Id, c.Name, Seats = c.TotalSeats })
                .ToListAsync();

            return Json(cinemas);
        }

        private bool CinemaExists(int id)
        {
            return _context.Cinemas.Any(e => e.Id == id);
        }

        private async Task LoadDropdownData(Cinema? cinema = null)
        {
            var theaters = await _context.Theaters
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.TheaterId = new SelectList(theaters, "Id", "Name", cinema?.TheaterId);
        }

        // Fix UpdateTheaterCapacity to work with computed property
        private async Task UpdateTheaterCapacity(int theaterId)
        {
            // Không cần update TotalCapacity vì nó là computed property
            // Chỉ cần lưu thay đổi của Cinema
        }
    }
}