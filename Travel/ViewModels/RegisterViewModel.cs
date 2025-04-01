using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(RegisterViewModel), "AgeValidation")]
        public DateTime? DateOfBirth { get; set; }

        public static ValidationResult AgeValidation(DateTime? dateOfBirth, ValidationContext context)
        {
            if (!dateOfBirth.HasValue)
            {
                return new ValidationResult("Ngày sinh là bắt buộc.");
            }

            DateTime today = DateTime.Today;

            // 🔹 Kiểm tra nếu ngày sinh lớn hơn ngày hiện tại
            if (dateOfBirth.Value.Date > today)
            {
                return new ValidationResult("Ngày sinh không thể lớn hơn ngày hiện tại.");
            }

            int age = today.Year - dateOfBirth.Value.Year;

            if (dateOfBirth.Value.Date > today.AddYears(-age))
            {
                age--;
            }
            if (age < 18)
            {
                return new ValidationResult("Bạn phải đủ 18 tuổi.");
            }
            if (age > 100)
            {
                return new ValidationResult("Ngày sinh không hợp lệ.");
            }

            return ValidationResult.Success;
        }
    }
}
