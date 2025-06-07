using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Theater)
                .OrderByDescending(t => t.BookingDate)
                .ToListAsync();
            return View(tickets);
        }

        // GET: Tickets/Book/5 (ShowtimeId)
        public async Task<IActionResult> Book(int? showtimeId)
        {
            if (showtimeId == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            // Kiểm tra xem suất chiếu đã qua chưa
            var showtimeDateTime = showtime.Date.Date.Add(showtime.Time);
            if (showtimeDateTime < DateTime.Now)
            {
                TempData["Error"] = "Cannot book tickets for past showtimes.";
                return RedirectToAction("Details", "Showtimes", new { id = showtimeId });
            }

            // Kiểm tra ghế còn trống
            if (showtime.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats for this showtime.";
                return RedirectToAction("Details", "Showtimes", new { id = showtimeId });
            }

            // Tạo danh sách ghế đã đặt
            var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
            ViewBag.BookedSeats = bookedSeats;

            return View(showtime);
        }

        // POST: Tickets/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book([Bind("ShowtimeId,UserName,Email,Phone,SeatNumber,Quantity")] BookTicketViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var showtime = await _context.Showtimes
                        .Include(s => s.Movie)
                        .Include(s => s.Theater)
                        .Include(s => s.Tickets)
                        .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId);

                    if (showtime == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra ghế có sẵn
                    if (showtime.AvailableSeats < model.Quantity)
                    {
                        ModelState.AddModelError("Quantity", "Not enough available seats.");
                        ViewBag.BookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
                        return View(showtime);
                    }

                    // Kiểm tra ghế đã được đặt chưa
                    var requestedSeats = model.SeatNumber.Split(',').Select(s => s.Trim()).ToList();
                    var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
                    var conflictSeats = requestedSeats.Intersect(bookedSeats).ToList();

                    if (conflictSeats.Any())
                    {
                        ModelState.AddModelError("SeatNumber", $"Seats {string.Join(", ", conflictSeats)} are already booked.");
                        ViewBag.BookedSeats = bookedSeats;
                        return View(showtime);
                    }

                    // Tạo vé cho từng ghế
                    var tickets = new List<Ticket>();
                    foreach (var seat in requestedSeats)
                    {
                        var ticket = new Ticket
                        {
                            ShowtimeId = model.ShowtimeId,
                            UserId = User.Identity?.Name ?? "Anonymous", // Sử dụng UserId thay vì UserName
                            SeatNumber = seat, // Sử dụng SeatNumber thay vì Seat
                            Price = showtime.Movie.Price,
                            Status = "Booked", // Sử dụng status phù hợp
                            BookingDate = DateTime.Now, // Sử dụng BookingDate thay vì CreatedAt
                            BookingReference = $"BK{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}"
                        };
                        tickets.Add(ticket);
                    }

                    // Lưu vé và cập nhật số ghế trống
                    _context.Tickets.AddRange(tickets);
                    showtime.AvailableSeats -= model.Quantity;
                    
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Successfully booked {model.Quantity} ticket(s) for {showtime.Movie.Title}!";
                    return RedirectToAction("Confirmation", new { id = tickets.First().Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error booking ticket: {ex.Message}");
                TempData["Error"] = "An error occurred while booking tickets.";
            }

            // Reload data if error
            var showtimeReload = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId);

            ViewBag.BookedSeats = showtimeReload?.Tickets.Select(t => t.SeatNumber).ToList() ?? new List<string>();
            return View(showtimeReload);
        }

        // GET: Tickets/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Showtime)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Kiểm tra thời gian hủy (ví dụ: chỉ được hủy trước 2 giờ)
            var showtimeDateTime = ticket.Showtime.Date.Date.Add(ticket.Showtime.Time);
            if (showtimeDateTime.AddHours(-2) < DateTime.Now)
            {
                TempData["Error"] = "Cannot cancel ticket less than 2 hours before showtime.";
                return RedirectToAction("Details", new { id = id });
            }

            ticket.Status = "Cancelled";
            ticket.Showtime.AvailableSeats += 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket cancelled successfully!";
            return RedirectToAction("Index");
        }
    }
}
