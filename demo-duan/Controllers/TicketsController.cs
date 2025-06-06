using demo_duan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Book(int showtimeId)
        {
            var showtime = db.Showtimes.Include("Movie").FirstOrDefault(s => s.ShowtimeId == showtimeId);
            return View(showtime);
        }

        [HttpPost]
        public ActionResult Book(int showtimeId, string customerName, int seats)
        {
            var ticket = new Ticket
            {
                ShowtimeId = showtimeId,
                CustomerName = customerName,
                Seats = seats
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();
            return RedirectToAction("Success");
        }

        public ActionResult Success()
        {
            return View();
        }
    }
}
