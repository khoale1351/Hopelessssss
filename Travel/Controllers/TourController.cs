using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Data;
//using Travel.Migrations;
using Travel.Models;

namespace Travel.Controllers
{
    [Authorize]
    public class TourController : Controller
    {
        private readonly TourismDbContext _context;

        public TourController(TourismDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var tours = _context.Tours.ToList();
            return View(tours);
        }

        // Hiển thị trang đặt tour
        public IActionResult Book()
        {
            return View();
        }

        // Xử lý đặt tour (POST)
        [HttpPost]
        public IActionResult Book(string tour, string name, string email, string phone)
        {
            // Lưu thông tin đặt tour (ví dụ: vào database)
            // Ở đây chỉ là ví dụ đơn giản, bạn có thể thêm logic xử lý thực tế
            ViewBag.Message = $"Thank you, {name}! Your booking for Tour ID {tour} has been received.";
            return View();
        }

        public IActionResult Details(int tourId)
        {
            var tour = _context.Tours.Find(tourId);
            return View(tour);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(Tour tour)
        {
            if (ModelState.IsValid)
            {
                tour.CreatedAt = DateTime.UtcNow;
                _context.Tours.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, Tour tour)
        {
            if (id != tour.TourId)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                _context.Tours.Update(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Delete(int id)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}