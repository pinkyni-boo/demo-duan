using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using demo_duan.ViewModels;
using Microsoft.AspNetCore.Identity;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, 
                              ILogger<HomeController> logger,
                              UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Home - Trang chủ
        public async Task<IActionResult> Index()
        {
            try
            {
                var movies = await _context.Movies
                    .Include(m => m.Category)
                    .Where(m => m.IsActive && m.Status == MovieStatus.NowShowing)
                    .OrderByDescending(m => m.ReleaseDate)
                    .Take(12)
                    .ToListAsync();

                var categories = await _context.Categories
                    .Where(c => c.Movies.Any(m => m.IsActive))
                    .ToListAsync();

                // Sửa dòng 84 - kiểm tra null an toàn
                var featuredMovies = movies
                    .Where(m => m.Category != null && m.Category.Name != null && m.Category.Name.Equals("Hành động", StringComparison.OrdinalIgnoreCase))
                    .Take(6)
                    .ToList();

                ViewBag.Categories = categories;
                ViewBag.FeaturedMovies = featuredMovies;

                return View(movies);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu trang chủ: " + ex.Message;
                return View(new List<Movie>());
            }
        }

        // GET: Home/Movies - Tất cả phim cho người dùng
        public async Task<IActionResult> Movies(int? categoryId, string searchTerm)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = searchTerm;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.ShowDate >= DateTime.Today)) // Sửa s.Date thành s.ShowDate
                .ThenInclude(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .AsQueryable();

            // Lọc theo category
            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchTerm))
            {
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchTerm) || 
                                                    m.Description.Contains(searchTerm));
            }

            var movies = await moviesQuery
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "All Movies";
            return View(movies);
        }

        // GET: Home/NowShowing - Phim đang chiếu
        public async Task<IActionResult> NowShowing(int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.ShowDate >= DateTime.Today)) // Sửa s.Date thành s.ShowDate
                .ThenInclude(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .Where(m => m.Showtimes.Any(s => s.ShowDate >= DateTime.Today)) // Sửa s.Date thành s.ShowDate
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            var movies = await moviesQuery
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "Now Showing";
            return View("Movies", movies);
        }

        // GET: Home/ComingSoon - Phim sắp chiếu
        public async Task<IActionResult> ComingSoon(int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .ThenInclude(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .Where(m => m.ReleaseDate > DateTime.Today || 
                           !m.Showtimes.Any(s => s.ShowDate >= DateTime.Today)) // Sửa s.Date thành s.ShowDate
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId);
            }

            var movies = await moviesQuery
                .OrderBy(m => m.ReleaseDate)
                .ToListAsync();

            ViewData["Title"] = "Coming Soon";
            return View("Movies", movies);
        }

        // GET: Home/MovieDetails/5
        public async Task<IActionResult> MovieDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.ShowDate >= DateTime.Today)) // Sửa s.Date thành s.ShowDate
                .ThenInclude(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Home/BookTicket/5 - Yêu cầu đăng nhập
        [Authorize]
        public async Task<IActionResult> BookTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Cần tách thành 2 bước vì AvailableSeats là computed property
            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes.Where(s => s.ShowDate >= DateTime.Today)) // Chỉ lọc theo ShowDate
                .ThenInclude(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .Include(m => m.Showtimes.Where(s => s.ShowDate >= DateTime.Today)) // Cần include lại Showtimes để load Tickets
                .ThenInclude(s => s.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Lọc các suất chiếu có ghế trống trong bộ nhớ
            var availableShowtimes = movie.Showtimes.Where(s => s.AvailableSeats > 0).ToList();
            // Gán lại danh sách showtimes đã lọc
            movie.Showtimes = availableShowtimes;

            if (!movie.Showtimes.Any())
            {
                TempData["Error"] = "No available showtimes for this movie.";
                return RedirectToAction("MovieDetails", new { id = id });
            }

            return View(movie);
        }

        // GET: Home/SelectSeats/5 - Yêu cầu đăng nhập
        [Authorize]
        public async Task<IActionResult> SelectSeats(int? showtimeId)
        {
            if (showtimeId == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .ThenInclude(m => m.Category)
                .Include(s => s.Cinema)
                .ThenInclude(c => c.Theater) // Sửa lại cách include Theater
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            var showtimeDateTime = showtime.ShowDate.Date.Add(showtime.ShowTime); // Sửa Date thành ShowDate
            if (showtimeDateTime < DateTime.Now)
            {
                TempData["Error"] = "Cannot book tickets for past showtimes.";
                return RedirectToAction("BookTicket", new { id = showtime.MovieId });
            }

            if (showtime.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats for this showtime.";
                return RedirectToAction("BookTicket", new { id = showtime.MovieId });
            }

            var bookedSeats = showtime.Tickets.Select(t => t.SeatNumber).ToList();
            ViewBag.BookedSeats = bookedSeats;

            return View(showtime);
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int showtimeId, string selectedSeats)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                return RedirectToAction("SelectSeats", new { showtimeId });
            }
            
            // Lấy thông tin suất chiếu
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);
                
            if (showtime == null)
            {
                return NotFound();
            }
            
            // Lấy danh sách phương thức thanh toán
            var paymentMethods = await _context.PaymentMethods
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
            
            // Xử lý danh sách ghế đã chọn
            var seatList = selectedSeats.Split(',').ToList();
            
            // Tạo view model
            var viewModel = new PaymentViewModel
            {
                ShowtimeId = showtimeId,
                MovieTitle = showtime.Movie.Title,
                ShowDate = showtime.ShowDate, // Hoặc showtime.ShowTime.Date
                ShowTime = showtime.ShowTime, // Không sử dụng .TimeOfDay
                TheaterName = showtime.Cinema.Theater.Name,
                CinemaName = showtime.Cinema.Name,
                SelectedSeats = seatList,
                TicketPrice = showtime.Price,
                AvailablePaymentMethods = paymentMethods,
                SelectedPaymentMethodId = paymentMethods.FirstOrDefault()?.Id ?? 0
            };
            
            // Nếu user đã đăng nhập, điền thông tin sẵn
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.CustomerName = user.FullName;
                    viewModel.CustomerEmail = user.Email;
                    viewModel.CustomerPhone = user.PhoneNumber;
                }
            }
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Lấy lại danh sách phương thức thanh toán
                model.AvailablePaymentMethods = await _context.PaymentMethods
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();
                    
                return View("Payment", model);
            }
            
            try
            {
                // Lấy thông tin suất chiếu
                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Cinema)
                    .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId);
                    
                if (showtime == null)
                {
                    return NotFound();
                }
                
                // Tạo mã đơn hàng ngẫu nhiên
                string bookingCode = GenerateBookingCode();
                
                // Tạo đơn đặt vé mới
                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    ShowtimeId = model.ShowtimeId,
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    BookingDate = DateTime.Now,
                    TotalAmount = model.TotalAmount,
                    PaymentStatus = "Pending",
                    IsActive = true
                };
                
                // Nếu user đã đăng nhập, liên kết với tài khoản
                if (User.Identity?.IsAuthenticated == true)
                {
                    booking.UserId = _userManager.GetUserId(User);
                }
                
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                // Tạo chi tiết vé cho từng ghế
                foreach (var seat in model.SelectedSeats)
                {
                    var ticket = new Ticket
                    {
                        BookingId = booking.Id,
                        SeatCode = seat,
                        Price = showtime.Price,
                        IsActive = true,
                        // Các thuộc tính khác
                    };
                    
                    _context.Tickets.Add(ticket);
                }
                
                await _context.SaveChangesAsync();
                
                // Lấy thông tin phương thức thanh toán
                var paymentMethod = await _context.PaymentMethods.FindAsync(model.SelectedPaymentMethodId);
                if (paymentMethod == null)
                {
                    TempData["ErrorMessage"] = "Phương thức thanh toán không hợp lệ";
                    return RedirectToAction("Payment", new { showtimeId = model.ShowtimeId, selectedSeats = string.Join(",", model.SelectedSeats) });
                }
                
                // Tạo thanh toán mới cho mỗi vé
                var tickets = await _context.Tickets.Where(t => t.BookingId == booking.Id).ToListAsync();
                foreach (var ticket in tickets)
                {
                    // Tickets có thể chưa có Payments (có thể là null)
                    if (ticket.Payments != null)
                    {
                        foreach (var p in ticket.Payments)
                        {
                            // Xử lý payments
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                
                // Cập nhật trạng thái thanh toán
                if (paymentMethod.Name.Contains("Trực tiếp") || paymentMethod.Name.Contains("Tại quầy"))
                {
                    // Thanh toán tại quầy - giữ trạng thái Pending
                    return RedirectToAction("BookingConfirmation", new { bookingCode });
                }
                else
                {
                    // Chuyển hướng đến trang thanh toán online (ví dụ: VNPay, Momo,...)
                    return RedirectToAction("ProcessPayment", new { bookingCode });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message;
                return RedirectToAction("Payment", new { showtimeId = model.ShowtimeId, selectedSeats = string.Join(",", model.SelectedSeats) });
            }
        }

        // Phương thức xử lý thanh toán online
        [HttpGet]
        public async Task<IActionResult> ProcessPayment(string bookingCode)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tickets)!
                    .ThenInclude(t => t.Payments)!
                    .ThenInclude(p => p.PaymentMethod)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);
            
            if (booking == null)
            {
                return NotFound();
            }
            
            // Lấy payment method từ payment đầu tiên
            var payment = booking.Tickets.FirstOrDefault()?.Payments.FirstOrDefault();
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán";
                return RedirectToAction("Index");
            }
            
            // TODO: Tích hợp cổng thanh toán thực tế (VNPay, Momo,...)
            // Đây là mô phỏng thanh toán thành công
            
            // Cập nhật trạng thái thanh toán thành công
            foreach (var ticket in booking.Tickets)
            {
                foreach (var p in ticket.Payments)
                {
                    p.Status = PaymentStatus.Completed;
                    p.ProcessedDate = DateTime.Now;
                    p.Notes = "Thanh toán thành công qua cổng thanh toán";
                }
            }
            
            booking.PaymentStatus = "Completed";
            await _context.SaveChangesAsync();
            
            return RedirectToAction("BookingConfirmation", new { bookingCode });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }

        // Thêm action method
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Phương thức helper để tạo mã đơn hàng
        private string GenerateBookingCode()
        {
            // Kết hợp thời gian hiện tại và số ngẫu nhiên để tạo mã độc nhất
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string random = new Random().Next(1000, 9999).ToString();
            return "BK" + timestamp + random;
        }
    }
}