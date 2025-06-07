using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace demo_duan.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _context.Tickets
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Cinema)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.BookingDate)
                .ToListAsync();

            return View(tickets);
        }

        // GET: Tickets/Book/5
        public async Task<IActionResult> Book(int? id)
        {
            if (id == null) return NotFound();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Cinema)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null) return NotFound();

            // Get booked seats
            var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
            ViewBag.BookedSeats = bookedSeats;
            
            return View(showtime);
        }

        // POST: Tickets/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int showtimeId, List<int> selectedSeats)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (selectedSeats == null || !selectedSeats.Any())
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một ghế.";
                    return RedirectToAction(nameof(Book), new { id = showtimeId });
                }

                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Cinema)
                    .Include(s => s.Tickets)
                    .FirstOrDefaultAsync(s => s.Id == showtimeId);

                if (showtime == null)
                {
                    TempData["ErrorMessage"] = "Suất chiếu không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if selected seats are already booked
                var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
                var conflictSeats = selectedSeats.Where(s => bookedSeats.Contains(s)).ToList();

                if (conflictSeats.Any())
                {
                    TempData["ErrorMessage"] = $"Ghế số {string.Join(", ", conflictSeats)} đã được đặt.";
                    return RedirectToAction(nameof(Book), new { id = showtimeId });
                }

                // Check seat availability
                if (selectedSeats.Count > showtime.AvailableSeats)
                {
                    TempData["ErrorMessage"] = "Không đủ ghế trống.";
                    return RedirectToAction(nameof(Book), new { id = showtimeId });
                }

                // Create tickets
                var tickets = new List<Ticket>();
                foreach (var seat in selectedSeats)
                {
                    var ticket = new Ticket
                    {
                        ShowtimeId = showtimeId,
                        UserId = userId,
                        SeatNumber = seat,
                        TotalPrice = showtime.Price,
                        BookingDate = DateTime.Now,
                        Status = "Booked",
                        TicketCode = GenerateTicketCode()
                    };
                    tickets.Add(ticket);
                }

                _context.Tickets.AddRange(tickets);

                // Update available seats
                showtime.AvailableSeats -= selectedSeats.Count;
                _context.Update(showtime);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đặt vé thành công cho {selectedSeats.Count} ghế!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đặt vé: " + ex.Message;
                return RedirectToAction(nameof(Book), new { id = showtimeId });
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Cinema)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Check if user owns this ticket
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ticket.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(ticket);
        }

        // POST: Tickets/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Showtime)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    TempData["ErrorMessage"] = "Vé không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if user owns this ticket
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (ticket.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Check if showtime has already started
                var showtimeStart = ticket.Showtime.Date.Add(ticket.Showtime.Time);
                if (showtimeStart <= DateTime.Now.AddHours(2)) // Must cancel at least 2 hours before
                {
                    TempData["ErrorMessage"] = "Không thể hủy vé. Suất chiếu bắt đầu trong vòng 2 giờ.";
                    return RedirectToAction(nameof(Index));
                }

                // Update ticket status
                ticket.Status = "Cancelled";
                _context.Update(ticket);

                // Update available seats
                ticket.Showtime.AvailableSeats += 1;
                _context.Update(ticket.Showtime);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hủy vé thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi hủy vé: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private string GenerateTicketCode()
        {
            return "TK" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
        }
    }
}
