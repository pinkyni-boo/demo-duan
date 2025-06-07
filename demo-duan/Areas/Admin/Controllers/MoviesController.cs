using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();
            return View(movies);
        }

        // GET: Admin/Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Cinema)
                        .ThenInclude(c => c.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // GET: Admin/Movies/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: Admin/Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Duration,ReleaseDate,Price,Img,CategoryId,Language,Director,Cast,TrailerUrl,IsActive")] Movie movie)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Showtimes");
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    // Sửa dòng 62 - không gán string cho enum
                    movie.UpdateStatusBasedOnReleaseDate(); // Method này sẽ set Status
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Phim đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tạo phim: " + ex.Message;
                }
            }

            await LoadDropdownData(movie);
            return View(movie);
        }

        // GET: Admin/Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            await LoadDropdownData(movie);
            return View(movie);
        }

        // POST: Admin/Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Duration,ReleaseDate,Price,Img,CategoryId,Language,Director,Cast,TrailerUrl,IsActive")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("Showtimes");
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    movie.UpdateStatusBasedOnReleaseDate();
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Phim đã được cập nhật! Trạng thái: {movie.StatusDisplayName}";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                }
            }

            await LoadDropdownData(movie);
            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Phim đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        private async Task LoadDropdownData(Movie? movie = null)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", movie?.CategoryId);
        }
    }
}
