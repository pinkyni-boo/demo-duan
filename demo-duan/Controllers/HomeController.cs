using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using demo_duan.ViewModels;
using Microsoft.AspNetCore.Identity;
using demo_duan.Areas.Identity.Data;
using System.Text;

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
        public async Task<IActionResult> BookTicket(int id, string date = null, int? theaterId = null)
        {
            // Lấy thông tin phim
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            // Xác định ngày
            DateTime selectedDate;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out selectedDate))
            {
                // Sử dụng ngày đã chọn
            }
            else
            {
                selectedDate = DateTime.Today;
            }
            
            // Log để debug
            _logger.LogInformation($"BookTicket - Movie: {id}, Date: {selectedDate:yyyy-MM-dd}, Theater: {theaterId}");
            
            // Lấy danh sách rạp có suất chiếu của phim này
            var theatersWithShowtimes = await _context.Theaters
                .Where(t => t.Cinemas.Any(c => c.Showtimes.Any(s => 
                    s.MovieId == id && 
                    s.ShowDate.Date >= DateTime.Today && 
                    s.IsActive)))
                .ToListAsync();
            
            // Nếu có chọn rạp và đã chọn ngày, load sẵn dữ liệu
            if (theaterId.HasValue)
            {
                var theaterShowtimes = await _context.Showtimes
                    .Include(s => s.Cinema)
                    .Include(s => s.Movie)
                    .Where(s => 
                        s.MovieId == id && 
                        s.Cinema.TheaterId == theaterId && 
                        s.ShowDate.Date == selectedDate.Date && 
                        s.IsActive)
                    .ToListAsync();
                    
                if (theaterShowtimes.Any())
                {
                    // Nhóm suất chiếu theo phòng
                    var groupedShowtimes = theaterShowtimes
                        .GroupBy(s => s.Cinema)
                        .Select(g => new
                        {
                            Cinema = g.Key,
                            Showtimes = g.ToList()
                        })
                        .ToList();
                        
                    ViewBag.SelectedTheaterId = theaterId;
                    ViewBag.SelectedDate = selectedDate;
                    ViewBag.GroupedShowtimes = groupedShowtimes;
                }
            }
            
            ViewBag.Theaters = theatersWithShowtimes;
            ViewBag.SelectedDate = selectedDate;
            
            return View(movie);
        }

        // GET: Home/SelectSeats/5 - Yêu cầu đăng nhập
        [Authorize]
        public async Task<IActionResult> SelectSeats(int id)
        {
            // Kiểm tra id hợp lệ
            if (id <= 0)
            {
                TempData["Error"] = "Mã suất chiếu không hợp lệ";
                return RedirectToAction("Index");
            }

            // Lấy thông tin suất chiếu kèm các bảng liên quan
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .ThenInclude(m => m.Category)
                .Include(s => s.Cinema)
                .ThenInclude(c => c.Theater)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
            {
                TempData["Error"] = "Không tìm thấy suất chiếu";
                return RedirectToAction("Index");
            }

            // Kiểm tra suất chiếu đã qua chưa
            if (showtime.ShowDate.Add(showtime.ShowTime) < DateTime.Now)
            {
                TempData["Error"] = "Suất chiếu này đã kết thúc, vui lòng chọn suất khác";
                return RedirectToAction("BookTicket", new { id = showtime.MovieId });
            }

            // Lấy danh sách ghế đã đặt
            var bookedSeats = showtime.Tickets
                .Where(t => t.Status != "Cancelled")
                .Select(t => t.SeatCode)
                .ToList();

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

        [HttpPost]
        public async Task<IActionResult> LoadShowtimes(int movieId, int theaterId, string date)
        {
            try
            {
                // Parse date
                DateTime selectedDate;
                if (!DateTime.TryParse(date, out selectedDate))
                {
                    selectedDate = DateTime.Today;
                }

                // Log thông tin
                _logger.LogInformation($"Loading showtimes for Movie: {movieId}, Theater: {theaterId}, Date: {selectedDate:yyyy-MM-dd}");

                // Truy vấn đơn giản nhất có thể để tránh lỗi
                var showtimes = await _context.Showtimes
                    .Where(s => 
                        s.MovieId == movieId && 
                        s.Cinema.TheaterId == theaterId && 
                        s.ShowDate.Date == selectedDate.Date && 
                        s.IsActive)
                    .Include(s => s.Cinema)
                    .ToListAsync();

                // Nếu không có suất chiếu, trả về thông báo
                if (!showtimes.Any())
                {
                    return PartialView("_NoShowtimesPartial");
                }

                // Chuẩn bị dữ liệu theo cách an toàn nhất
                var viewModel = new List<object>();
                
                var groupedShowtimes = showtimes.GroupBy(s => s.Cinema);
                foreach (var group in groupedShowtimes)
                {
                    // Tạo anonymous object thay vì dùng dynamic để tránh lỗi
                    viewModel.Add(new
                    {
                        Cinema = new 
                        {
                            Id = group.Key.Id,
                            Name = group.Key.Name,
                            Type = group.Key.Type ?? "Standard",
                            TotalSeats = group.Key.TotalSeats
                        },
                        Showtimes = group.Select(s => new 
                        {
                            Id = s.Id,
                            ShowTime = s.ShowTime,
                            Price = s.Price,
                            AvailableSeats = s.Cinema.TotalSeats - (s.Tickets?.Count ?? 0)
                        }).ToList()
                    });
                }

                return PartialView("_ShowtimesPartial", viewModel);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                _logger.LogError(ex, "Error in LoadShowtimes");
                
                // Trả về partial view với thông báo lỗi
                return PartialView("_ErrorPartial", "Chi tiết lỗi: " + ex.Message);
            }
        }

        // Thêm phương thức này để kiểm tra dữ liệu
        [HttpGet]
        [Route("Home/DebugShowtimes/{movieId}")]
        public async Task<IActionResult> DebugShowtimes(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                return Content("Movie not found");
            }
            
            var result = new StringBuilder();
            result.AppendLine($"Movie: {movie.Title} (ID: {movie.Id})");
            
            // Kiểm tra có các showtimes nào cho phim này
            var showtimes = await _context.Showtimes
                .Include(s => s.Cinema)
                    .ThenInclude(c => c.Theater)
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.ShowDate)
                .ThenBy(s => s.ShowTime)
                .ToListAsync();
            
            result.AppendLine($"Total showtimes: {showtimes.Count}");
            
            // Kiểm tra các showtime từ hôm nay trở đi
            var currentShowtimes = showtimes.Where(s => s.ShowDate.Date >= DateTime.Today).ToList();
            result.AppendLine($"Future showtimes: {currentShowtimes.Count}");
            
            // Liệt kê chi tiết của 10 showtime đầu tiên
            result.AppendLine("Details of up to 10 showtimes:");
            foreach (var showtime in currentShowtimes.Take(10))
            {
                result.AppendLine($"- ID: {showtime.Id}, Date: {showtime.ShowDate:yyyy-MM-dd}, Time: {showtime.ShowTime:hh\\:mm}");
                result.AppendLine($"  Theater: {showtime.Cinema.Theater.Name} (ID: {showtime.Cinema.TheaterId})");
                result.AppendLine($"  Cinema: {showtime.Cinema.Name} (ID: {showtime.CinemaId})");
                result.AppendLine($"  IsActive: {showtime.IsActive}, Status: {showtime.Status}");
            }
            
            return Content(result.ToString(), "text/plain");
        }

        // Thêm phương thức debug này
        [HttpGet]
        [Route("Home/TestShowtimes/{movieId}/{theaterId}")]
        public async Task<IActionResult> TestShowtimes(int movieId, int theaterId)
        {
            // Kiểm tra phim
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                return Content($"Movie with ID {movieId} not found");
            }
            
            // Kiểm tra rạp
            var theater = await _context.Theaters.FindAsync(theaterId);
            if (theater == null)
            {
                return Content($"Theater with ID {theaterId} not found");
            }
            
            // Kiểm tra cinema của rạp
            var cinemas = await _context.Cinemas
                .Where(c => c.TheaterId == theaterId)
                .ToListAsync();
    
            // Kiểm tra showtime
            var showtimes = await _context.Showtimes
                .Include(s => s.Cinema)
                .Where(s => 
                    s.MovieId == movieId && 
                    s.Cinema.TheaterId == theaterId && 
                    s.ShowDate.Date >= DateTime.Today && 
                    s.IsActive)
                .ToListAsync();
    
            // Build kết quả
            var result = $"Movie: {movie.Title} (ID: {movie.Id})\n" +
                         $"Theater: {theater.Name} (ID: {theater.Id})\n" +
                         $"Cinemas count: {cinemas.Count}\n" +
                         $"Showtimes count: {showtimes.Count}\n\n";
    
            if (showtimes.Any())
            {
                result += "Showtimes:\n";
                foreach(var showtime in showtimes)
                {
                    result += $"- ID: {showtime.Id}, Date: {showtime.ShowDate:yyyy-MM-dd}, " +
                              $"Time: {showtime.ShowTime}, Cinema: {showtime.Cinema.Name}\n";
                }
            }
            else
            {
                result += "No showtimes found for this movie at this theater.";
            }
    
            return Content(result);
        }

        [HttpGet]
        [Route("Home/ShowtimesDebug/{movieId}/{theaterId}/{date}")]
        public async Task<IActionResult> ShowtimesDebug(int movieId, int theaterId, string date)
        {
            try
            {
                // Parse date
                DateTime selectedDate;
                if (!DateTime.TryParse(date, out selectedDate))
                {
                    selectedDate = DateTime.Today;
                }

                // Kiểm tra phim
                var movie = await _context.Movies.FindAsync(movieId);
                if (movie == null)
                {
                    return Content($"Phim có ID {movieId} không tồn tại");
                }
                
                // Kiểm tra rạp
                var theater = await _context.Theaters.FindAsync(theaterId);
                if (theater == null)
                {
                    return Content($"Rạp có ID {theaterId} không tồn tại");
                }
                
                // Kiểm tra rạp có phòng không
                var cinemas = await _context.Cinemas
                    .Where(c => c.TheaterId == theaterId)
                    .ToListAsync();
                if (!cinemas.Any())
                {
                    return Content($"Rạp {theater.Name} không có phòng chiếu nào");
                }

                // Kiểm tra suất chiếu
                var showtimes = await _context.Showtimes
                    .Include(s => s.Cinema)
                    .Where(s => 
                        s.MovieId == movieId && 
                        s.Cinema.TheaterId == theaterId && 
                        s.ShowDate.Date == selectedDate.Date)
                    .ToListAsync();
                
                var activeShowtimes = showtimes.Where(s => s.IsActive).ToList();

                var result = $"Thông tin debug:\n" +
                             $"- Phim: {movie.Title} (ID: {movie.Id})\n" +
                             $"- Rạp: {theater.Name} (ID: {theater.Id})\n" +
                             $"- Ngày: {selectedDate:yyyy-MM-dd}\n" +
                             $"- Số phòng chiếu: {cinemas.Count}\n" +
                             $"- Tổng suất chiếu: {showtimes.Count}\n" +
                             $"- Suất chiếu đang hoạt động: {activeShowtimes.Count}\n\n";

                if (activeShowtimes.Any())
                {
                    result += "Chi tiết suất chiếu:\n";
                    foreach (var s in activeShowtimes)
                    {
                        result += $"- ID: {s.Id}, Phòng: {s.Cinema.Name}, Giờ: {s.ShowTime}\n";
                    }
                }
                else
                {
                    result += "Không có suất chiếu nào đang hoạt động cho phim này tại rạp này vào ngày đã chọn.\n";
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}\n\nStack Trace: {ex.StackTrace}");
            }
        }

        [HttpGet]
        [Route("Home/DbTest/{movieId}/{theaterId}/{date}")]
        public async Task<IActionResult> DbTest(int movieId, int theaterId, string date)
        {
            try
            {
                // Parse date
                DateTime selectedDate;
                if (!DateTime.TryParse(date, out selectedDate))
                {
                    selectedDate = DateTime.Today;
                }
                
                // Kiểm tra từng bước để xác định vấn đề
                var movie = await _context.Movies.FindAsync(movieId);
                var theater = await _context.Theaters.FindAsync(theaterId);
                
                var cinemas = await _context.Cinemas
                    .Where(c => c.TheaterId == theaterId)
                    .ToListAsync();
                    
                var showtimes = await _context.Showtimes
                    .Where(s => s.MovieId == movieId)
                    .ToListAsync();
                    
                var filteredShowtimes = showtimes
                    .Where(s => s.Cinema.TheaterId == theaterId && s.ShowDate.Date == selectedDate.Date && s.IsActive)
                    .ToList();
                
                // Trả về kết quả dưới dạng JSON để kiểm tra
                return Json(new {
                    movie = movie != null ? new { movie.Id, movie.Title } : null,
                    theater = theater != null ? new { theater.Id, theater.Name } : null,
                    cinemasCount = cinemas.Count,
                    cinemaIds = cinemas.Select(c => c.Id),
                    showtimesTotal = showtimes.Count,
                    filteredCount = filteredShowtimes.Count,
                    filteredIds = filteredShowtimes.Select(s => s.Id)
                });
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
    }
}