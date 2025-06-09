using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TheatersController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public TheatersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Theaters
        public async Task<IActionResult> Index()
        {
            var theaters = await _context.Theaters
                .Include(t => t.Cinemas) // Changed from Showtimes to Cinemas
                .ToListAsync();
            return View(theaters);
        }

        // GET: Admin/Theaters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var theater = await _context.Theaters
                .Include(t => t.Cinemas) // Changed from Showtimes
                    .ThenInclude(c => c.Showtimes)
                        .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (theater == null) return NotFound();

            return View(theater);
        }

        // GET: Admin/Theaters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Theaters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,City,Phone,Email")] Theater theater) // Updated properties
        {
            if (ModelState.IsValid)
            {
                _context.Add(theater);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Rạp '{theater.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(theater);
        }

        // GET: Admin/Theaters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var theater = await _context.Theaters.FindAsync(id);
            if (theater == null) return NotFound();

            return View(theater);
        }

        // POST: Admin/Theaters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,City,Phone,Email,IsActive")] Theater theater) // Updated properties
        {
            if (id != theater.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(theater);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Rạp '{theater.Name}' đã được cập nhật!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TheaterExists(theater.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(theater);
        }

        // GET: Admin/Theaters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var theater = await _context.Theaters
                .Include(t => t.Cinemas) // Changed from Showtimes to Cinemas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (theater == null) return NotFound();

            return View(theater);
        }

        // POST: Admin/Theaters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var theater = await _context.Theaters.FindAsync(id);
            if (theater != null)
            {
                _context.Theaters.Remove(theater);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Rạp '{theater.Name}' đã được xóa!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TheaterExists(int id)
        {
            return _context.Theaters.Any(e => e.Id == id);
        }

        // Thêm vào phương thức Create (GET)
        // (Removed duplicate Create action to resolve method conflict)
    }
}