using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Movie)
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Cinema)
                            .ThenInclude(c => c.Theater)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            return View(payments);
        }

        // GET: Admin/Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Movie)
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Cinema)
                            .ThenInclude(c => c.Theater)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // GET: Admin/Payments/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: Admin/Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,PaymentMethodId,UserId,Amount,Status,TransactionId,Notes")] Payment payment)
        {
            ModelState.Remove("Ticket");
            ModelState.Remove("PaymentMethod");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    payment.PaymentDate = DateTime.Now;
                    
                    // Sửa dòng 91 - không gán string cho enum
                    if (payment.Status == PaymentStatus.Completed)
                    {
                        payment.ProcessedDate = DateTime.Now;
                    }

                    // Generate transaction ID if not provided
                    if (string.IsNullOrEmpty(payment.TransactionId))
                    {
                        payment.TransactionId = $"TXN_{DateTime.Now:yyyyMMddHHmmss}_{payment.TicketId}";
                    }

                    _context.Add(payment);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Thanh toán đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tạo thanh toán: " + ex.Message;
                }
            }

            await LoadDropdownData(payment);
            return View(payment);
        }

        // GET: Admin/Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            await LoadDropdownData(payment);
            return View(payment);
        }

        // POST: Admin/Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,PaymentMethodId,UserId,Amount,Status,PaymentDate,TransactionId,Notes")] Payment payment)
        {
            if (id != payment.Id) return NotFound();

            ModelState.Remove("Ticket");
            ModelState.Remove("PaymentMethod");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update processed date when status changes to completed
                    var existingPayment = await _context.Payments.AsNoTracking().FirstAsync(p => p.Id == id);
                    
                    if (existingPayment.Status != PaymentStatus.Completed && payment.Status == PaymentStatus.Completed)
                    {
                        payment.ProcessedDate = DateTime.Now;
                    }
                    else if (payment.Status != PaymentStatus.Completed)
                    {
                        payment.ProcessedDate = null;
                    }
                    else
                    {
                        payment.ProcessedDate = existingPayment.ProcessedDate;
                    }

                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Thanh toán đã được cập nhật!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                }
            }

            await LoadDropdownData(payment);
            return View(payment);
        }

        // GET: Admin/Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Movie)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Admin/Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thanh toán đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API Methods
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, PaymentStatus status)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                    return Json(new { success = false, message = "Không tìm thấy thanh toán" });

                payment.Status = status;
                
                if (status == PaymentStatus.Completed)
                {
                    payment.ProcessedDate = DateTime.Now;
                }
                else if (status == PaymentStatus.Failed || status == PaymentStatus.Cancelled)
                {
                    payment.ProcessedDate = null;
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Cập nhật trạng thái thành công",
                    newStatus = GetStatusDisplayName(status),
                    statusClass = GetStatusBadgeClass(status)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(p => p.Id == id);
        }

        private async Task LoadDropdownData(Payment? payment = null)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Movie)
                .Include(t => t.Showtime)
                .Where(t => t.Status == "Booked")
                .ToListAsync();

            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.DisplayOrder)
                .ToListAsync();

            var users = await _context.Users.ToListAsync();

            ViewBag.TicketId = new SelectList(
                tickets.Select(t => new { t.Id, Display = $"Vé #{t.Id} - {t.Movie.Title} - {t.SeatNumber}" }), 
                "Id", "Display", payment?.TicketId
            );

            ViewBag.PaymentMethodId = new SelectList(paymentMethods, "Id", "Name", payment?.PaymentMethodId);
            ViewBag.UserId = new SelectList(users, "Id", "Email", payment?.UserId);

            // Payment Status options
            var statusOptions = Enum.GetValues<PaymentStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) })
                .ToList();
            
            ViewBag.StatusOptions = new SelectList(statusOptions, "Value", "Text", (int?)payment?.Status);
        }

        private static string GetStatusDisplayName(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Chờ thanh toán",
                PaymentStatus.Completed => "Đã thanh toán",
                PaymentStatus.Failed => "Thất bại",
                PaymentStatus.Cancelled => "Đã hủy",
                PaymentStatus.Refunded => "Đã hoàn tiền",
                _ => "Không xác định"
            };
        }

        private static string GetStatusBadgeClass(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "bg-warning",
                PaymentStatus.Completed => "bg-success",
                PaymentStatus.Failed => "bg-danger",
                PaymentStatus.Cancelled => "bg-secondary",
                PaymentStatus.Refunded => "bg-info",
                _ => "bg-secondary"
            };
        }
    }
}