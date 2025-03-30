using Microsoft.AspNetCore.Mvc;
using Travel.Data;
using Travel.Models;

namespace Travel.Controllers
{
    public class PaymentController : Controller
    {
        private readonly TourismDbContext _context;
        public PaymentController(TourismDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách thanh toán của người dùng hiện tại
        public IActionResult Index()
        {
            var userId = User.Identity.Name;
            var payments = _context.Payments.Where(p => p.UserId == userId).ToList();
            return View(payments);
        }

        // Form thanh toán cho một booking cụ thể
        public IActionResult ProcessPayment(int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound();
            }
            ViewBag.Booking = booking;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int bookingId, Payment payment)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                payment.BookingId = bookingId;
                payment.UserId = User.Identity.Name;
                payment.TransactionDate = DateTime.UtcNow;
                payment.PaymentStatus = "Paid";
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }
    }
}
