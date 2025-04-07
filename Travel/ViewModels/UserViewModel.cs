using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Họ và tên không được dài quá 255 ký tự.")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Email không được dài quá 255 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Địa chỉ không được dài quá 255 ký tự.")]
        public string Address { get; set; }

        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ. Phải có 10-11 chữ số và bắt đầu bằng 0.")]
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; }

        public string MembershipType { get; set; } = "Silver";
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; } = true;

        public IFormFile? AvatarFile { get; set; }

        public string? Avatar { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
    }
}
