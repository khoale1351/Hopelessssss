using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestPDF.Fluent;
using SixLabors.ImageSharp.Formats.Jpeg;
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
using X.PagedList.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using DocumentFormat.OpenXml.InkML;
using Travel.Repositories.IMAGESERVICE;
using Travel.Data;

namespace Travel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private readonly IImageService _imageService;
        private readonly TourismDbContext _context;

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMemoryCache cache, IImageService imageService, TourismDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
            _imageService = imageService;
            _context = context;
        }

//================================ Trang Tổng quan Dasboard =====================================================
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
        public IActionResult IndexAdmin()
        {
            return View();
        }

//================================ Quản lý Người dùng (User) =====================================================
        public async Task<IActionResult> ManageUsers(string searchQuery, string roleFilter, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var usersQuery = _userManager.Users.AsQueryable();
            //var users = await _userManager.Users.ToListAsync();
            //var roles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FullName.ToLower().Contains(searchQuery) 
                    || (u.Email != null && u.Email.ToLower().Contains(searchQuery)));
            }

            var allUsers = await usersQuery.ToListAsync();

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                var filteredUsers = new List<ApplicationUser>();
                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, roleFilter))
                    {
                        filteredUsers.Add(user);
                    }
                }
                //users = users.Where(u => _userManager.IsInRoleAsync(u, roleFilter).Result).ToList();
                allUsers = filteredUsers;
            }

            //ViewBag.Roles = roles;
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.SelectedRole = roleFilter;
            ViewBag.SearchQuery = searchQuery;

            var pagedUsers = allUsers.ToPagedList(pageNumber, pageSize);
            return View("Users/ManageUsers", pagedUsers);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Id không hợp lệ");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng");

            var roles = await _userManager.GetRolesAsync(user);

            // Cập nhật ProfilePictureUrl dựa trên AvatarPath (nếu rỗng thì dùng ảnh mặc định)
            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                MembershipType = user.MembershipType,
                Status = user.Status,
                ProfilePictureUrl = string.IsNullOrEmpty(user.AvatarPath) ? "/images/default-avatar.png" : user.AvatarPath
            };

            return View("Users/UserDetails", viewModel);
        }


        public IActionResult CreateUser()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = new SelectList(roles);
            return View("Users/CreateUser");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model, string Role)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                ViewBag.Roles = new SelectList(roles);
                return View("Users/CreateUser", model);
            }

            string? avatarPath = null;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var avatarResult = await _imageService.SaveImageAsync(
                    model.AvatarFile,
                    "images/avatars",
                    filePrefix: "user",
                    targetSize: new Size(200, 200) // Kích thước cần resize cho avatar
                );

                if (!avatarResult.IsSuccess)
                {
                    ModelState.AddModelError("AvatarFile", avatarResult.ErrorMessage);
                    var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                    ViewBag.Roles = new SelectList(roles);
                    return View("Users/CreateUser", model);
                }

                avatarPath = avatarResult.FilePath;
            }

            // Kiểm tra tên có được nhập
            if (string.IsNullOrEmpty(model.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và Tên không để trống!");
                return View("Users/CreateUser", model);
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email không để trống!");
                return View("Users/CreateUser", model);
            }

            // Kiểm tra Email hợp lệ
            if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View("Users/CreateUser", model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
                return View("Users/CreateUser", model);
            }

            // Kiểm tra số điện thoại hợp lệ (10-11 chữ số, bắt đầu bằng 0)
            if (!string.IsNullOrEmpty(model.PhoneNumber) && !Regex.IsMatch(model.PhoneNumber, @"^0\d{9,10}$"))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ. Phải có 10-11 chữ số và bắt đầu bằng 0.");
                return View("Users/CreateUser", model);
            }

            // Kiểm tra ngày sinh hợp lệ
            if (model.DateOfBirth.HasValue)
            {
                DateTime today = DateTime.Today;
                int age = today.Year - model.DateOfBirth.Value.Year;
                if (model.DateOfBirth.Value.Date > today.AddYears(-age)) age--;

                if (age < 18)
                {
                    ModelState.AddModelError("DateOfBirth", "Người dùng phải từ 18 tuổi trở lên.");
                    return View("Users/CreateUser", model);
                }
                if (age > 100)
                {
                    ModelState.AddModelError("DateOfBirth", "Ngày sinh không hợp lệ.");
                    return View("Users/CreateUser", model);
                }
                if (model.DateOfBirth.Value.Date == today.Date)
                {
                    ModelState.AddModelError("DateOfBirth", "Ngày sinh không thể là ngày hiện tại.");
                    return View("Users/CreateUser", model);
                }
            }
            
            // Kiểm tra mật khẩu (chỉ cần tối thiểu 8 ký tự)
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự.");
                return View("Users/CreateUser", model);
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
                    CreatedAt = DateTime.UtcNow,
                    AvatarPath = avatarPath
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

            return View("Users/CreateUser", model);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.Status = "Inactive";
                user.IsActive = false;
            }
            else
            {
                user.Status = "Active";
                user.IsActive = true;
            }
            var model = new UserViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                MembershipType = user.MembershipType,
                Status = user.Status,
                IsActive = user.IsActive,
                Role = userRoles.FirstOrDefault()
            };
            ViewBag.RoleList = new SelectList(allRoles, model.Role);
            await LoadEditUserViewBags(user);
            return View("Users/EditUser", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            string? oldAvatarPath = user.AvatarPath;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var avatarResult = await _imageService.SaveImageAsync(
                    model.AvatarFile,
                    "images/avatars",
                    filePrefix: "user",
                    oldFilePath: oldAvatarPath,
                    targetSize: new Size(200, 200)
                );
                if (!avatarResult.IsSuccess)
                {
                    ModelState.AddModelError("AvatarFile", avatarResult.ErrorMessage);
                    await LoadEditUserViewBags(user);
                    return View("Users/EditUser", model);
                }

                user.AvatarPath = avatarResult.FilePath;
            }

            // Cập nhật thông tin người dùng
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.MembershipType = model.MembershipType;

            // Cập nhật vai trò người dùng
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadEditUserViewBags(user);
            return View("Users/EditUser", model);
        }


        private async Task LoadEditUserViewBags(ApplicationUser user)
        {
            ViewBag.UserId = user.Id;
            ViewBag.CurrentAvatar = user.AvatarPath;

            // Cập nhật danh sách vai trò từ DB thay vì hardcode
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.RoleList = new SelectList(allRoles, userRoles.FirstOrDefault());

            // Trạng thái: Kích hoạt / Vô hiệu hóa (hardcoded)
            var statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Kích hoạt", Selected = (user.Status == "Active") },
                new SelectListItem { Value = "Inactive", Text = "Vô hiệu hóa", Selected = (user.Status == "Inactive") }
            };
            ViewBag.StatusList = statusList;

            // MembershipType (hardcoded)
            ViewBag.MembershipTypes = new SelectList(new[] {
                new SelectListItem { Value = "Silver", Text = "Silver" },
                new SelectListItem { Value = "Gold", Text = "Gold" },
                new SelectListItem { Value = "Platinum", Text = "Platinum" },
                new SelectListItem { Value = "Diamond", Text = "Diamond" }
            }, "Value", "Text", user.MembershipType);
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
            user.IsActive = false;
            user.Status = "Inactive";

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
            user.Status = "Active";

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể mở khóa tài khoản.");
            }

            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ExportUserToPdf(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Chi tiết người dùng: {user.FullName ?? "N/A"}")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(user.AvatarPath))
                        {
                            try
                            {
                                byte[] imageBytes = System.IO.File.ReadAllBytes(Path.Combine("wwwroot", user.AvatarPath));

                                col.Item().PaddingBottom(10).AlignCenter().Element(container =>
                                {
                                    container
                                        .Height(150)
                                        .Width(150)
                                        .Image(imageBytes)
                                        .FitArea(); // Fix lỗi layout
                                });
                            }
                            catch
                            {
                                col.Item().Text("Ảnh đại diện: N/A").Italic();
                            }
                        }
                        else
                        {
                            col.Item().Text("Ảnh đại diện: N/A").Italic();
                        }



                        col.Item().Text($"Họ và tên: {user.FullName ?? "N/A"}");
                        col.Item().Text($"Email: {user.Email ?? "N/A"}");
                        col.Item().Text($"Số điện thoại: {user.PhoneNumber ?? "N/A"}");
                        col.Item().Text($"Ngày sinh: {user.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A"}");
                        col.Item().Text($"Địa chỉ: {user.Address ?? "N/A"}");
                        col.Item().Text($"Loại thành viên: {user.MembershipType ?? "N/A"}");
                        col.Item().Text($"Trạng thái: {(user.IsActive ? "Hoạt động" : "Đã khóa")}");
                        col.Item().Text($"Ngày tạo: {user.CreatedAt.ToString("dd/MM/yyyy")}");
                        col.Item().Text($"Vai trò: {(roles.Count > 0 ? string.Join(", ", roles) : "N/A")}");
                    });
                });
            });

            using var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", $"{user.FullName ?? "User"}_Details.pdf");
        }

        public async Task<IActionResult> ExportAllUsersToPdf()
        {
            var users = await _userManager.Users.ToListAsync();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Danh sách người dùng").FontSize(20).Bold().AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // FullName
                            columns.RelativeColumn(); // Email
                            columns.RelativeColumn(); // Phone
                            columns.ConstantColumn(100); // Trạng thái
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Họ và tên").Bold();
                            header.Cell().Text("Email").Bold();
                            header.Cell().Text("Số điện thoại").Bold();
                            header.Cell().Text("Trạng thái").Bold();
                        });

                        foreach (var user in users)
                        {
                            table.Cell().Text(user.FullName ?? "N/A");
                            table.Cell().Text(user.Email ?? "N/A");
                            table.Cell().Text(user.PhoneNumber ?? "N/A");
                            table.Cell().Text(user.IsActive ? "Hoạt động" : "Đã khóa");
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", "Users_List.pdf");
        }

        public async Task<IActionResult> ExportUsersToExcel()
        {
            var users = await _userManager.Users.ToListAsync();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Header
            worksheet.Cell(1, 1).Value = "Họ và tên";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Điện thoại";
            worksheet.Cell(1, 4).Value = "Trạng thái";
            worksheet.Cell(1, 5).Value = "Ngày tạo";

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                worksheet.Cell(i + 2, 1).Value = user.FullName ?? "N/A";
                worksheet.Cell(i + 2, 2).Value = user.Email ?? "N/A";
                worksheet.Cell(i + 2, 3).Value = user.PhoneNumber ?? "N/A";
                worksheet.Cell(i + 2, 4).Value = user.IsActive ? "Hoạt động" : "Đã khóa";
                worksheet.Cell(i + 2, 5).Value = user.CreatedAt.ToString("dd/MM/yyyy");
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        }


        [HttpPost]
        public async Task<IActionResult> IsEmailUnique(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Json(new { exists = user != null });
        }

/*================================ Quản lý Tour ==========================================================*/
        public async Task<IActionResult> ManageTours()
        {
            var tours = await _unitOfWork.Tours.GetAllAsync();
            return View(tours);
        }

        //[HttpGet]
        //public async Task<IActionResult> SearchDestinations(string searchTerm)
        //{
        //    try
        //    {
        //        var destinations = await _unitOfWork.Destinations.GetAllAsync();

        //        var results = destinations
        //            .Where(d => string.IsNullOrEmpty(searchTerm) ||
        //                        d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        //                        (d.City != null && d.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
        //                        (d.Country != null && d.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
        //            .Select(d => new
        //            {
        //                destinationId = d.DestinationId,
        //                name = d.Name,
        //                city = d.City,
        //                country = d.Country
        //            })
        //            .Take(50)
        //            .ToList();

        //        return Json(results);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "Lỗi khi tải điểm đến: " + ex.Message });
        //    }
        //}

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    return View(new TourViewModel());
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(TourViewModel model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var newTour = new Tour
        //            {
        //                TourName = model.TourName,
        //                Description = model.Description,
        //                Price = model.Price,
        //                Duration = model.Duration,
        //                StartDate = model.StartDate,
        //                EndDate = model.EndDate,
        //                AvailableSeats = model.AvailableSeats,
        //                TourType = model.TourType,
        //                TourStatus = model.TourStatus,
        //                DestinationId = model.DestinationId,
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            await _unitOfWork.Tours.AddAsync(newTour);
        //            await _unitOfWork.SaveChangesAsync();

        //            TempData["SuccessMessage"] = "Tour created successfully!";
        //            return RedirectToAction("Index");
        //        }

        //        // Nếu có lỗi validation, load lại danh sách điểm đến
        //        model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
        //            .Select(d => new SelectListItem
        //            {
        //                Value = d.DestinationId.ToString(),
        //                Text = d.Name
        //            }).ToList();
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", "Error creating tour: " + ex.Message);
        //        model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
        //            .Select(d => new SelectListItem
        //            {
        //                Value = d.DestinationId.ToString(),
        //                Text = d.Name
        //            }).ToList();
        //        return View(model);
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetTourDetails(int id)
        //{
        //    var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        //    if (tour == null) return NotFound();

        //    // Load thông tin liên quan nếu cần
        //    tour.Destination = await _unitOfWork.Destinations.GetByIdAsync(tour.DestinationId);

        //    return PartialView("_TourDetailPartial", tour);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetEditTourForm(int id)
        //{
        //    var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        //    if (tour == null) return NotFound();

        //    // Load danh sách điểm đến cho dropdown
        //    ViewBag.Destinations = await _unitOfWork.Destinations.GetAllAsync();

        //    return PartialView("_EditTourPartial", tour);
        //}

        //[HttpPost]
        //public async Task<IActionResult> EditTour(
        //    int TourId, string TourName, int DestinationId, string Description, decimal Price, int Duration,
        //    DateTime StartDate, DateTime EndDate,
        //    int AvailableSeats, string TourType, string TourStatus, IFormFile ImageFile)
        //{
        //    try
        //    {
        //        var existingTour = await _unitOfWork.Tours.GetByIdAsync(TourId);
        //        if (existingTour == null)
        //        {
        //            return NotFound();
        //        }

        //        // Cập nhật thông tin
        //        existingTour.TourName = TourName;
        //        existingTour.DestinationId = DestinationId;
        //        existingTour.Description = Description;
        //        existingTour.Price = Price;
        //        existingTour.Duration = Duration;
        //        existingTour.StartDate = StartDate;
        //        existingTour.EndDate = EndDate;
        //        existingTour.AvailableSeats = AvailableSeats;
        //        existingTour.TourType = TourType;
        //        existingTour.TourStatus = TourStatus;

        //        // Xử lý ảnh
        //        if (ImageFile != null && ImageFile.Length > 0)
        //        {
        //            // Đảm bảo thư mục uploads tồn tại
        //            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        //            if (!Directory.Exists(uploadsDir))
        //            {
        //                Directory.CreateDirectory(uploadsDir);
        //            }

        //            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //            var filePath = Path.Combine(uploadsDir, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await ImageFile.CopyToAsync(stream);
        //            }

        //            existingTour.ImageUrl = "/uploads/" + fileName;
        //        }

        //        await _unitOfWork.Tours.UpdateAsync(existingTour);
        //        await _unitOfWork.SaveChangesAsync();

        //        return Json(new { redirect = Url.Action("ManageTours") });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Lỗi khi cập nhật tour: " + ex.Message);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteTour(int id)
        //{
        //    try
        //    {
        //        // Kiểm tra xem tour có tồn tại không
        //        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        //        if (tour == null)
        //        {
        //            return Json(new { success = false, message = "Tour không tồn tại." });
        //        }

        //        // Xóa các booking liên quan (nếu có)
        //        var bookings = await _unitOfWork.Bookings.GetAllAsync();
        //        var relatedBookings = bookings.Where(b => b.TourId == id).ToList();
        //        foreach (var booking in relatedBookings)
        //        {
        //            await _unitOfWork.Bookings.DeleteAsync(booking.BookingId);
        //        }

        //        // Xóa các đánh giá liên quan (nếu có)
        //        var reviews = await _unitOfWork.Reviews.GetAllAsync();
        //        var relatedReviews = reviews.Where(r => r.TourId == id).ToList();
        //        foreach (var review in relatedReviews)
        //        {
        //            await _unitOfWork.Reviews.DeleteAsync(review.ReviewId);
        //        }

        //        // Xóa tour
        //        await _unitOfWork.Tours.DeleteAsync(id);
        //        await _unitOfWork.SaveChangesAsync();

        //        return Json(new { success = true, message = "Xóa tour thành công!", redirect = Url.Action("ManageTours") });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi khi xóa tour: " + ex.Message });
        //    }
        //}
/*========================== Quản lý Đặt Tour (Booking) ====================================================*/
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            return View(bookings);
        }

/*================================ Quản lý Đánh giá (Review) ===============================================*/
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync();
            return View(reviews);
        }

/*================================ Quản lý Voucher =========================================================*/
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

        public async Task<IActionResult> ManageForumCategories()
        {
            var categories = await _context.ForumCategories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateForumCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForumCategory(ForumCategory category)
        {
            if (ModelState.IsValid)
            {
                await _context.ForumCategories.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageForumCategories));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditForumCategory(int id)
        {
            var category = await _context.ForumCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForumCategory(int id, ForumCategory category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageForumCategories));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ForumCategories.Any(e => e.CategoryId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForumCategory(int id)
        {
            var category = await _context.ForumCategories.FindAsync(id);
            if (category != null)
            {
                _context.ForumCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageForumCategories));
        }
    }
}