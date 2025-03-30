using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Models;
using Travel.ViewModels;

namespace Travel.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Hiển thị trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure:true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // Hiển thị trang đăng ký
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra nếu email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                return View(model);
            }

            // Kiểm tra tên đầy đủ không được để trống
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và tên không được để trống.");
            }

            // Kiểm tra ngày sinh hợp lệ
            if (model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError("DateOfBirth", "Ngày sinh không hợp lệ.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                MembershipType = "Silver",
                Status = "Active",
                IsActive = true,
                AccessFailedCount = 0
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Kiểm tra role "Customer" đã tồn tại chưa, nếu chưa thì tạo
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                // Gán quyền mặc định "Customer"
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError("Password", "Mật khẩu phải chứa ít nhất 8 ký tự.");
                    }
                    else if (error.Code == "ConfirmPasswordMismatch")
                    {
                            ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Hiển thị trang quên mật khẩu
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Xử lý quên mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            // Gửi email (tích hợp dịch vụ email ở đây)
            System.Diagnostics.Debug.WriteLine($"Link đặt lại mật khẩu: {resetLink}");

            return View("ForgotPasswordConfirmation");
        }

        // Hiển thị trang đặt lại mật khẩu
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // Xử lý đặt lại mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản không hợp lệ hoặc đã bị khóa.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation"); // Không cần RedirectToAction
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // Hiển thị thông báo đặt lại mật khẩu thành công
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
