using Microsoft.AspNetCore.Mvc;

namespace Travel.Controllers
{
    public class AccountController : Controller
    {
        // Hiển thị trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Kiểm tra thông tin đăng nhập
            if (email == "user@example.com" && password == "password")
            {
                // Lưu thông tin đăng nhập (ví dụ: sử dụng session hoặc cookie)
                HttpContext.Session.SetString("UserEmail", email);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction("Index", "Home");
        }
    }
}