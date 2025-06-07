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
                .OrderByDescending(m => m.Id)
                .ToListAsync();
            return View(movies);
        }

        // GET: Admin/Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .ThenInclude(s => s.Theater)
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Admin/Movies/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load categories for dropdown
                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
                
                // Create new movie with default values
                var movie = new Movie
                {
                    ReleaseDate = DateTime.Today,
                    Status = "Sắp chiếu"
                };
                
                return View(movie);
            }
            catch (Exception ex)
            {
                // Log error
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo phim: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Duration,Img,Price,ReleaseDate,CategoryId")] Movie movie)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set auto-generated properties
                    movie.UpdateStatusBasedOnReleaseDate();
                    
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Phim '{movie.Title}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo phim: " + ex.Message;
            }

            // Reload categories if validation fails
            var categories = await _context.Categories.ToListAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", movie.CategoryId);
            return View(movie);
        }

        // GET: Admin/Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", movie.CategoryId);
            return View(movie);
        }

        // POST: Admin/Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Duration,Img,Price,ReleaseDate,CategoryId")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tự động cập nhật Status dựa trên Release Date
                    movie.UpdateStatusBasedOnReleaseDate();
                    
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Phim '{movie.Title}' đã được cập nhật với trạng thái: {movie.Status}";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", movie.CategoryId);
            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Showtimes)
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (movie != null)
            {
                // Kiểm tra có showtimes và tickets không
                if (movie.Showtimes.Any() || movie.Tickets.Any())
                {
                    TempData["Error"] = "Cannot delete movie with existing showtimes or tickets.";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Movie deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
