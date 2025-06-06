using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;
using demo_duan.Models;
using Microsoft.AspNetCore.Authorization;

namespace demo_duan.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Ticket)
                .Include(p => p.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .ThenInclude(t => t.Movie)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Process/5 (Process payment for ticket)
        public async Task<IActionResult> Process(int? ticketId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Movie)
                .Include(t => t.Showtime)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            var payment = new Payment
            {
                TicketId = ticket.Id,
                Amount = ticket.TotalPrice,
                UserId = ticket.UserId
            };

            ViewBag.PaymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .ToListAsync();

            return View(payment);
        }

        // POST: Payments/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process([Bind("TicketId,Amount,PaymentMethod,CardHolderName,CardNumber,BankName,Notes")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentDate = DateTime.Now;
                payment.CreatedAt = DateTime.Now;
                payment.TransactionId = GenerateTransactionId();
                payment.PaymentStatus = "Completed"; // In real app, this would be set by payment gateway
                
                _context.Add(payment);
                
                // Update ticket status
                var ticket = await _context.Tickets.FindAsync(payment.TicketId);
                if (ticket != null)
                {
                    ticket.Status = "Confirmed";
                    _context.Update(ticket);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Success", new { id = payment.Id });
            }

            ViewBag.PaymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .ToListAsync();
            
            return View(payment);
        }

        // GET: Payments/Success/5
        public async Task<IActionResult> Success(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .ThenInclude(t => t.Movie)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        private string GenerateTransactionId()
        {
            return "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }
    }
}