using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Data;
using Travel.Models;

namespace Travel.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly TourismDbContext _context;
        public BookingController(TourismDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.Identity.Name; // hoặc lấy từ ClaimTypes.NameIdentifier
            var bookings = _context.Bookings.Where(b => b.UserId == userId).ToList();
            return View(bookings);
        }

        public IActionResult Create(int tourId)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.TourId == tourId);
            if (tour == null)
            {
                return NotFound();
            }
            ViewBag.Tour = tour;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.UserId = User.Identity.Name;
                booking.BookingDate = DateTime.UtcNow;
                booking.Status = "Pending";
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        public IActionResult Details(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // Hủy booking
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }
            // Cập nhật trạng thái hủy booking
            booking.Status = "Cancelled";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
