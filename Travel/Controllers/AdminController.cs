using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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
                    u.Email != null && u.Email.ToLower().Contains(searchQuery))
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
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = new SelectList(roles);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model, string Role)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔹 Kiểm tra Email hợp lệ
            if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(model);
            }

            // 🔹 Kiểm tra số điện thoại hợp lệ (10-11 chữ số, bắt đầu bằng 0)
            if (!Regex.IsMatch(model.PhoneNumber, @"^0\d{9,10}$"))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ. Phải có 10-11 chữ số và bắt đầu bằng 0.");
                return View(model);
            }

            // 🔹 Kiểm tra ngày sinh hợp lệ
            if (!model.DateOfBirth.HasValue)
            {
                ModelState.AddModelError("DateOfBirth", "Ngày sinh là bắt buộc.");
                return View(model);
            }

            DateTime today = DateTime.Today;
            int age = today.Year - model.DateOfBirth.Value.Year;
            if (model.DateOfBirth.Value.Date > today.AddYears(-age)) age--;

            if (age < 18)
            {
                ModelState.AddModelError("DateOfBirth", "Người dùng phải từ 18 tuổi trở lên.");
                return View(model);
            }
            if (age > 100)
            {
                ModelState.AddModelError("DateOfBirth", "Ngày sinh không hợp lệ.");
                return View(model);
            }

            // 🔹 Kiểm tra mật khẩu (chỉ cần tối thiểu 8 ký tự)
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự.");
                return View(model);
            }

            try
            {
                // Tạo đối tượng người dùng từ model
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
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Tạo tài khoản người dùng với mật khẩu
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán role cho người dùng
                    if (!string.IsNullOrEmpty(Role))
                    {
                        if (!await _roleManager.RoleExistsAsync(Role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(Role));
                        }
                        await _userManager.AddToRoleAsync(user, Role);
                    }
                    else
                    {
                        // Nếu không chọn role, gán role mặc định
                        const string defaultRole = "Customer";
                        await _userManager.AddToRoleAsync(user, defaultRole);
                    }

                    TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                    return RedirectToAction("ManageUsers");
                }

                // Xử lý lỗi khi tạo tài khoản
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm người dùng: " + ex.Message);
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

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
            user.IsActive = false; // Cập nhật trạng thái người dùng

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể khóa tài khoản.");
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            user.IsActive = true; 

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể mở khóa tài khoản.");
            }

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
                var destinations = await _unitOfWork.Destinations.GetAllAsync();

                var results = destinations
                    .Where(d => string.IsNullOrEmpty(searchTerm) ||
                                d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                (d.City != null && d.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                (d.Country != null && d.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .Select(d => new
                    {
                        destinationId = d.DestinationId,
                        name = d.Name,
                        city = d.City,
                        country = d.Country
                    })
                    .Take(50)
                    .ToList();

                return Json(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi tải điểm đến: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TourViewModel());
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
                model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                    .Select(d => new SelectListItem
                    {
                        Value = d.DestinationId.ToString(),
                        Text = d.Name
                    }).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating tour: " + ex.Message);
                model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                    .Select(d => new SelectListItem
                    {
                        Value = d.DestinationId.ToString(),
                        Text = d.Name
                    }).ToList();
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
                    // Đảm bảo thư mục uploads tồn tại
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTour(int id)
        {
            try
            {
                // Kiểm tra xem tour có tồn tại không
                var tour = await _unitOfWork.Tours.GetByIdAsync(id);
                if (tour == null)
                {
                    return Json(new { success = false, message = "Tour không tồn tại." });
                }

                // Xóa các booking liên quan (nếu có)
                var bookings = await _unitOfWork.Bookings.GetAllAsync();
                var relatedBookings = bookings.Where(b => b.TourId == id).ToList();
                foreach (var booking in relatedBookings)
                {
                    await _unitOfWork.Bookings.DeleteAsync(booking.BookingId);
                }

                // Xóa các đánh giá liên quan (nếu có)
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var relatedReviews = reviews.Where(r => r.TourId == id).ToList();
                foreach (var review in relatedReviews)
                {
                    await _unitOfWork.Reviews.DeleteAsync(review.ReviewId);
                }

                // Xóa tour
                await _unitOfWork.Tours.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa tour thành công!", redirect = Url.Action("ManageTours") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa tour: " + ex.Message });
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