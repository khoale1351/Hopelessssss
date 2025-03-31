using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Models;
using Travel.Repositories;
using Travel.ViewModels;

namespace Travel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin"); // Điều hướng Admin đến /Admin/Index
                    }
                    return RedirectToAction("Index", "Home"); // Người dùng thường đến /Home/Index
                }
                ModelState.AddModelError("", "Đăng nhập thất bại.");
            }
            return View(model);
        }

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Management() // Đổi từ "Managerment" thành "Management"
        {
            return View("Management"); // Trả về Views/Admin/Admin.cshtml
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _unitOfWork.Users.GetAllAsync();
            var totalTours = await _unitOfWork.Tours.GetAllAsync();
            var totalBookings = await _unitOfWork.Bookings.GetAllAsync();
            var totalReviews = await _unitOfWork.Reviews.GetAllAsync();
            ViewBag.TotalUsers = totalUsers.Count();
            ViewBag.TotalTours = totalTours.Count();
            ViewBag.TotalBookings = totalBookings.Count();
            ViewBag.TotalReviews = totalReviews.Count();
            return View();
        }

        public IActionResult ManageUsers()
        {
            var users = _unitOfWork.Users.GetAllAsync();
            return View(users);
        }

        public IActionResult ManageTours()
        {
            var tours = _unitOfWork.Tours.GetAllAsync();
            return View(tours);
        }


        public IActionResult ManageBookings()
        {
            var bookings = _unitOfWork.Bookings.GetAllAsync();
            return View(bookings);
        }

        public IActionResult ManageReviews()
        {
            var reviews = _unitOfWork.Reviews.GetAllAsync();
            return View(reviews);
        }
    }
}
