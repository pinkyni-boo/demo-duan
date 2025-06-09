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
            // Kiểm tra và load danh sách rạp
            var theaters = await _context.Theaters
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
                
            // DEBUG: Kiểm tra xem có rạp nào không
            if (!theaters.Any())
            {
                TempData["ErrorMessage"] = "Không có rạp chiếu phim nào. Vui lòng tạo rạp trước.";
                return RedirectToAction("Index", "Theaters");
            }
            
            // Đảm bảo ViewBag.TheaterId được thiết lập đúng
            ViewBag.TheaterId = new SelectList(theaters, "Id", "Name");
            return View();
        }

        // POST: Admin/Cinemas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinema cinema)
        {
            // Quan trọng: Loại bỏ validation cho navigation properties
            ModelState.Remove("Theater");
            ModelState.Remove("Showtimes");

            try
            {
                if (ModelState.IsValid)
                {
                    // Đảm bảo tính toán TotalSeats dựa trên Rows và SeatsPerRow
                    cinema.TotalSeats = cinema.Rows * cinema.SeatsPerRow;
                    cinema.CreatedDate = DateTime.Now;
                    
                    _context.Cinemas.Add(cinema);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Thêm mới phòng chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Hiển thị chi tiết lỗi
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["ErrorMessage"] = $"Dữ liệu không hợp lệ: {errors}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }
            
            // Tải lại danh sách rạp
            var theaters = await _context.Theaters
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.TheaterId = new SelectList(theaters, "Id", "Name", cinema.TheaterId);
            
            return View(cinema);
        }

        // GET: Admin/Cinemas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            
            await LoadDropdownData(cinema);
            return View(cinema);
        }

        // POST: Admin/Cinemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinema cinema)
        {
            if (id != cinema.Id)
            {
                return NotFound();
            }

            // Loại bỏ validation cho navigation properties
            ModelState.Remove("Theater");
            ModelState.Remove("Showtimes");

            if (ModelState.IsValid)
            {
                try
                {
                    // Tính lại TotalSeats
                    cinema.TotalSeats = cinema.Rows * cinema.SeatsPerRow;
                    cinema.UpdatedDate = DateTime.Now;
                    
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Phòng chiếu đã được cập nhật thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CinemaExists(cinema.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            await LoadDropdownData(cinema);
            return View(cinema);
        }

        // GET: Admin/Cinemas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .Include(c => c.Theater)
                .Include(c => c.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (cinema == null)
            {
                return NotFound();
            }

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
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
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

