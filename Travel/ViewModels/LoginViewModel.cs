using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
