using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;
using Microsoft.AspNetCore.Authorization;

namespace demo_duan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
                    .ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(p => p.PaymentMethod)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        // GET: Admin/Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Theater)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Admin/Payments/Create
        public async Task<IActionResult> Create()
        {
            ViewData["TicketId"] = new SelectList(
                await _context.Tickets
                    .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                    .Select(t => new { 
                        t.Id, 
                        DisplayText = $"{t.Showtime.Movie.Title} - {t.SeatNumber} - {t.BookingReference}"
                    })
                    .ToListAsync(), 
                "Id", "DisplayText");
            
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name");
            return View();
        }

        // POST: Admin/Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,PaymentMethodId,Amount,Status,TransactionId,Notes")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentDate = DateTime.Now;
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TicketId"] = new SelectList(
                await _context.Tickets
                    .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                    .Select(t => new { 
                        t.Id, 
                        DisplayText = $"{t.Showtime.Movie.Title} - {t.SeatNumber} - {t.BookingReference}"
                    })
                    .ToListAsync(), 
                "Id", "DisplayText", payment.TicketId);
            
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", payment.PaymentMethodId);
            return View(payment);
        }

        // GET: Admin/Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = new SelectList(
                await _context.Tickets
                    .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                    .Select(t => new { 
                        t.Id, 
                        DisplayText = $"{t.Showtime.Movie.Title} - {t.SeatNumber} - {t.BookingReference}"
                    })
                    .ToListAsync(), 
                "Id", "DisplayText", payment.TicketId);
            
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", payment.PaymentMethodId);
            return View(payment);
        }

        // POST: Admin/Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,PaymentMethodId,Amount,PaymentDate,Status,TransactionId,Notes")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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

            ViewData["TicketId"] = new SelectList(
                await _context.Tickets
                    .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                    .Select(t => new { 
                        t.Id, 
                        DisplayText = $"{t.Showtime.Movie.Title} - {t.SeatNumber} - {t.BookingReference}"
                    })
                    .ToListAsync(), 
                "Id", "DisplayText", payment.TicketId);
            
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", payment.PaymentMethodId);
            return View(payment);
        }

        // GET: Admin/Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}