using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Repositories;
using Microsoft.AspNetCore.Identity;
using Travel.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(ApplicationUser model, string password)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    MembershipType = model.MembershipType,
                    Status = model.Status,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer"); // Gán vai trò mặc định
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.IsLockedOut = await _userManager.IsLockedOutAsync(user);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Cập nhật tất cả các thuộc tính
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.MembershipType = model.MembershipType;
            user.Status = model.Status;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.IsLockedOut = await _userManager.IsLockedOutAsync(user);
            return View(model);
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
