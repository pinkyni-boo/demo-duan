using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Controllers
{
    public class TheatersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TheatersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Theaters
        public async Task<IActionResult> Index()
        {
            var theaters = await _context.Theaters
                .Include(t => t.Showtimes)
                .ToListAsync();
            return View(theaters);
        }

        // GET: Theaters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theater = await _context.Theaters
                .Include(t => t.Showtimes)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (theater == null)
            {
                return NotFound();
            }

            return View(theater);
        }

        // GET: Theaters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Theaters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Capacity,PhoneNumber,Email")] Theater theater)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(theater);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Theater created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating theater: {ex.Message}");
                TempData["Error"] = "An error occurred while creating the theater.";
            }
            
            return View(theater);
        }

        // GET: Theaters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theater = await _context.Theaters.FindAsync(id);
            if (theater == null)
            {
                return NotFound();
            }
            return View(theater);
        }

        // POST: Theaters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Capacity,PhoneNumber,Email")] Theater theater)
        {
            if (id != theater.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(theater);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Theater updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TheaterExists(theater.Id))
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
            return View(theater);
        }

        // GET: Theaters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theater = await _context.Theaters
                .Include(t => t.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (theater == null)
            {
                return NotFound();
            }

            return View(theater);
        }

        // POST: Theaters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var theater = await _context.Theaters
                .Include(t => t.Showtimes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (theater != null)
            {
                if (theater.Showtimes.Any())
                {
                    TempData["Error"] = "Cannot delete theater because it has scheduled showtimes. Please remove showtimes first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Theaters.Remove(theater);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Theater deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TheaterExists(int id)
        {
            return _context.Theaters.Any(e => e.Id == id);
        }

        // Seed sample theaters
        public async Task<IActionResult> SeedData()
        {
            try
            {
                if (!await _context.Theaters.AnyAsync())
                {
                    var theaters = new List<Theater>
                    {
                        new Theater 
                        { 
                            Name = "CGV Aeon Mall",
                            Address = "30 Bộ Lao, Phường 2, Tân Bình, TP.HCM",
                            Capacity = 150,
                            PhoneNumber = "(028) 7300 8881",
                            Email = "aeonmall@cgv.vn"
                        },
                        new Theater 
                        { 
                            Name = "CGV Vincom Center",
                            Address = "72 Lê Thánh Tôn, Phường Bến Nghé, Quận 1, TP.HCM",
                            Capacity = 200,
                            PhoneNumber = "(028) 7300 8882",
                            Email = "vincom@cgv.vn"
                        },
                        new Theater 
                        { 
                            Name = "Lotte Cinema Diamond",
                            Address = "34 Lê Duẩn, Phường Bến Nghé, Quận 1, TP.HCM",
                            Capacity = 180,
                            PhoneNumber = "(028) 3823 4567",
                            Email = "diamond@lottecinema.vn"
                        }
                    };

                    _context.Theaters.AddRange(theaters);
                    await _context.SaveChangesAsync();
                    
                    return Json(new { success = true, message = "Sample theaters created successfully" });
                }
                
                return Json(new { success = true, message = "Theaters already exist" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}