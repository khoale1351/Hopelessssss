using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using Travel.Models;
using Travel.Models.ViewModels;
using Travel.Repositories;

namespace Travel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
        }

        // Trang tổng quan Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var cacheKey = "DashboardData";
            if (!_cache.TryGetValue(cacheKey, out DashboardViewModel dashboardData))
            {
                try
                {
                    dashboardData = new DashboardViewModel
                    {
                        TotalUsers = await _unitOfWork.Users.CountAsync(),
                        TotalTours = await _unitOfWork.Tours.CountAsync(),
                        TotalBookings = await _unitOfWork.Bookings.CountAsync(),
                        TotalReviews = await _unitOfWork.Reviews.CountAsync(),

                        // Sửa lỗi bằng cách ép kiểu hoặc dùng LINQ sau khi lấy dữ liệu
                        RecentUsers = (await _unitOfWork.Users.GetAllAsync())
                            .OrderByDescending(u => u.CreatedAt)
                            .Take(5)
                            .ToList(),
                        RecentTours = (await _unitOfWork.Tours.GetAllAsync())
                            .AsQueryable()
                            .Include(t => t.Destination)
                            .OrderByDescending(t => t.CreatedAt)
                            .Take(5)
                            .ToList(),
                        RecentBookings = (await _unitOfWork.Bookings.GetAllAsync())
                            .AsQueryable()
                            .Include(b => b.Tour)
                            .Include(b => b.User)
                            .OrderByDescending(b => b.BookingDate)
                            .Take(5)
                            .ToList(),
                        RecentReviews = (await _unitOfWork.Reviews.GetAllAsync())
                            .AsQueryable()
                            .Include(r => r.Tour)
                            .Include(r => r.User)
                            .OrderByDescending(r => r.ReviewDate)
                            .Take(5)
                            .ToList()
                    };

                    _cache.Set(cacheKey, dashboardData, TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi tải dữ liệu: " + ex.Message);
                    return View(new DashboardViewModel());
                }
            }

            return View(dashboardData);
        }

        // Giữ nguyên Index cũ làm redirect
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // Quản lý người dùng
        public async Task<IActionResult> ManageUsers(string searchQuery, string roleFilter)
        {
            var users = await _userManager.Users.ToListAsync();
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(searchQuery) ||
                    u.Email.ToLower().Contains(searchQuery))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                users = users.Where(u => _userManager.IsInRoleAsync(u, roleFilter).Result).ToList();
            }

            ViewBag.Roles = roles;
            ViewBag.SelectedRole = roleFilter;
            ViewBag.SearchQuery = searchQuery;

            return View(users);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string id, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            return RedirectToAction("ManageUsers");
        }

        // Quản lý Tour
        public async Task<IActionResult> ManageTours()
        {
            var tours = await _unitOfWork.Tours.GetAllAsync();
            return View(tours);
        }

        // Quản lý đặt tour
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            return View(bookings);
        }

        // Quản lý đánh giá
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync();
            return View(reviews);
        }
    }
}