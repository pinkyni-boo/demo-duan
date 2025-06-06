using Microsoft.AspNetCore.Mvc;
using demo_duan.Data;
using demo_duan.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Where(m => m.ReleaseDate <= DateTime.Now)
                .OrderByDescending(m => m.ReleaseDate)
                .Take(6)
                .ToListAsync();
            
            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}