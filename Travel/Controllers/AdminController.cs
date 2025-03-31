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
using Travel.ViewModels;

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

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(ApplicationUser model, string password)
        {
            if (ModelState.IsValid)
            {
                try
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
                        // Gán role mặc định (ví dụ: "Customer")
                        await _userManager.AddToRoleAsync(user, "Customer");

                        TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                        return RedirectToAction("ManageUsers");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi thêm người dùng: " + ex.Message);
                }
            }

            // Nếu có lỗi, hiển thị lại form với thông báo lỗi
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
        [HttpGet]
        public async Task<IActionResult> SearchDestinations(string searchTerm)
        {
            try
            {
                // Lấy danh sách điểm đến phù hợp với từ khóa tìm kiếm
                var destinations = await _unitOfWork.Destinations.GetAllAsync();

                var filteredDestinations = destinations
                    .Where(d =>
                        d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        d.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        d.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(d => new
                    {
                        destinationId = d.DestinationId,
                        name = d.Name,
                        city = d.City,  // Sửa thành City
                        country = d.Country // Sửa thành Country
                    })
                    .ToList();

                return Json(filteredDestinations); // Sửa tên biến
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return StatusCode(500, new { error = "Lỗi khi tải điểm đến: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new TourViewModel
            {
                DestinationOptions = await _unitOfWork.Destinations.GetAllAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newTour = new Tour
                    {
                        TourName = model.TourName,
                        Description = model.Description,
                        Price = model.Price,
                        Duration = model.Duration,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        AvailableSeats = model.AvailableSeats,
                        TourType = model.TourType,
                        TourStatus = model.TourStatus,
                        DestinationId = model.DestinationId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Tours.AddAsync(newTour);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tour created successfully!";
                    return RedirectToAction("Index");
                }

                // Nếu có lỗi validation, load lại danh sách điểm đến
                model.DestinationOptions = await _unitOfWork.Destinations.GetAllAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating tour: " + ex.Message);
                model.DestinationOptions = await _unitOfWork.Destinations.GetAllAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTourDetails(int id)
        {
            var tour = await _unitOfWork.Tours.GetByIdAsync(id);
            if (tour == null) return NotFound();

            // Load thông tin liên quan nếu cần
            tour.Destination = await _unitOfWork.Destinations.GetByIdAsync(tour.DestinationId);

            return PartialView("_TourDetailPartial", tour);
        }

        [HttpGet]
        public async Task<IActionResult> GetEditTourForm(int id)
        {
            var tour = await _unitOfWork.Tours.GetByIdAsync(id);
            if (tour == null) return NotFound();

            // Load danh sách điểm đến cho dropdown
            ViewBag.Destinations = await _unitOfWork.Destinations.GetAllAsync();

            return PartialView("_EditTourPartial", tour);
        }

        [HttpPost]
        public async Task<IActionResult> EditTour(
    int TourId,
    string TourName,
    int DestinationId,
    string Description,
    decimal Price,
    int Duration,
    DateTime StartDate,
    DateTime EndDate,
    int AvailableSeats,
    string TourType,
    string TourStatus,
    IFormFile ImageFile)
        {
            try
            {
                var existingTour = await _unitOfWork.Tours.GetByIdAsync(TourId);
                if (existingTour == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin
                existingTour.TourName = TourName;
                existingTour.DestinationId = DestinationId;
                existingTour.Description = Description;
                existingTour.Price = Price;
                existingTour.Duration = Duration;
                existingTour.StartDate = StartDate;
                existingTour.EndDate = EndDate;
                existingTour.AvailableSeats = AvailableSeats;
                existingTour.TourType = TourType;
                existingTour.TourStatus = TourStatus;

                // Xử lý ảnh
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    existingTour.ImageUrl = "/uploads/" + fileName;
                }

                await _unitOfWork.Tours.UpdateAsync(existingTour);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { redirect = Url.Action("ManageTours") });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi cập nhật tour: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTour(int id)
        {
            try
            {
                await _unitOfWork.Tours.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa tour thành công!";
                return RedirectToAction("ManageTours");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa tour: " + ex.Message;
                return RedirectToAction("ManageTours");
            }
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

        // Quản lý Voucher
        public async Task<IActionResult> ManageVouchers()
        {
            var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
            return View(vouchers);
        }

        public async Task<IActionResult> DetailsVoucher(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        public IActionResult CreateVoucher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher([Bind("VoucherId,Code,Description,DiscountAmount,DiscountPercentage,MinimumBookingValue,MaxDiscountAmount,ExpiryDate,UsageLimit,UsageCount,IsActive")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Vouchers.AddAsync(voucher);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(ManageVouchers));
            }
            return View(voucher);
        }

        public async Task<IActionResult> EditVoucher(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVoucher(int id, [Bind("VoucherId,Code,Description,DiscountAmount,DiscountPercentage,MinimumBookingValue,MaxDiscountAmount,ExpiryDate,UsageLimit,UsageCount,IsActive")] Voucher voucher)
        {
            if (id != voucher.VoucherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.Vouchers.UpdateAsync(voucher);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _unitOfWork.Vouchers.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(ManageVouchers));
            }
            return View(voucher);
        }

        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost, ActionName("DeleteVoucher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVoucherConfirmed(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher != null)
            {
                await _unitOfWork.Vouchers.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageVouchers));
        }
    }
}