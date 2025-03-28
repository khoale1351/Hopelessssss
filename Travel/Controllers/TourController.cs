using Microsoft.AspNetCore.Mvc;

namespace Travel.Controllers
{
    public class TourController : Controller
    {
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
        public IActionResult Details()
        {
            return View();
        }
    }
}