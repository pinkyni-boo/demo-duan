using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demo_duan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiController> _logger;

        public ApiController(ApplicationDbContext context, ILogger<ApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("showtimes")]
        public async Task<ActionResult<IEnumerable<object>>> GetShowtimes(int movieId, int theaterId, string date)
        {
            try
            {
                // Parse date
                if (!DateTime.TryParse(date, out DateTime showDate))
                {
                    showDate = DateTime.Today;
                }
                
                // Log debug info
                _logger.LogInformation($"Getting showtimes for Movie: {movieId}, Theater: {theaterId}, Date: {showDate:yyyy-MM-dd}");

                // Query showtimes
                var showtimes = await _context.Showtimes
                    .Include(s => s.Cinema)
                    .Where(s => 
                        s.MovieId == movieId && 
                        s.Cinema.TheaterId == theaterId && 
                        s.ShowDate.Date == showDate.Date && 
                        s.IsActive)
                    .Select(s => new
                    {
                        id = s.Id,
                        cinemaId = s.CinemaId,
                        cinemaName = s.Cinema.Name,
                        cinemaType = s.Cinema.Type,
                        showTime = s.ShowTime.ToString(@"hh\:mm"),
                        availableSeats = s.AvailableSeats,
                        price = s.Price
                    })
                    .ToListAsync();
                
                _logger.LogInformation($"Found {showtimes.Count} showtimes for Movie: {movieId}, Theater: {theaterId}, Date: {showDate:yyyy-MM-dd}");
                
                return showtimes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving showtimes for Movie: {movieId}, Theater: {theaterId}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Thêm phương thức này vào ApiController để kiểm tra dữ liệu
        [HttpGet]
        [Route("debug/movie-showtimes/{movieId}")]
        public async Task<ActionResult<object>> DebugMovieShowtimes(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                return NotFound(new { error = "Movie not found" });
            }
            
            var showtimes = await _context.Showtimes
                .Where(s => s.MovieId == movieId && s.ShowDate >= DateTime.Today)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .ToListAsync();
                
            return new
            {
                movie = new { movie.Id, movie.Title },
                showtimesCount = showtimes.Count,
                showtimes = showtimes.Select(s => new
                {
                    id = s.Id,
                    theaterId = s.Cinema.TheaterId,
                    theaterName = s.Cinema.Theater.Name,
                    cinemaId = s.CinemaId, 
                    cinemaName = s.Cinema.Name,
                    date = s.ShowDate.ToString("yyyy-MM-dd"),
                    time = s.ShowTime.ToString(@"hh\:mm"),
                    isActive = s.IsActive
                })
            };
        }
    }
}