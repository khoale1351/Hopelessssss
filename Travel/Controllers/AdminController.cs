using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Repositories;
using Microsoft.AspNetCore.Identity;
using Travel.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Travel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _cache = cache;
        }

        //Trang tổng quan Dashboard
        public async Task<IActionResult> Index()
        {
            var cacheKey = "DashboardData";
            if (!_cache.TryGetValue(cacheKey, out (int users, int tours, int bookings, int reviews) dashboardData))
            {
                try
                {
                    var totalUsers = await _unitOfWork.Users.GetAllAsync();
                    var totalTours = await _unitOfWork.Tours.GetAllAsync();
                    var totalBookings = await _unitOfWork.Bookings.GetAllAsync();
                    var totalReviews = await _unitOfWork.Reviews.GetAllAsync();

                    dashboardData = (totalUsers.Count(), totalTours.Count(), totalBookings.Count(), totalReviews.Count());
                    _cache.Set(cacheKey, dashboardData, TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi tải dữ liệu: " + ex.Message);
                }
            }

            ViewBag.TotalUsers = dashboardData.users;
            ViewBag.TotalTours = dashboardData.tours;
            ViewBag.TotalBookings = dashboardData.bookings;
            ViewBag.TotalReviews = dashboardData.reviews;
            return View();
        }

        //Quản lý người dùng
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return View(users);
        }

        //Quản lý Tour
        public async Task<IActionResult> ManageTours()
        {

            var tours = await _unitOfWork.Tours.GetAllAsync();
            return View(tours);
        }

        //Quản lý đặt tour
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            return View(bookings);
        }
        
        //Quản lý đánh giá
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync();
            return View(reviews);
        }
    }
}
