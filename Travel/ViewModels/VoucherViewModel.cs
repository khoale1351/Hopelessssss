using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class VoucherViewModel
    {
        public int VoucherId { get; set; }
        [Required(ErrorMessage = "Mã voucher không được để trống.")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn 0.")]
        public decimal? DiscountAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải trong khoảng 0 đến 100.")]
        public decimal? DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đặt tour tối thiểu phải lớn hơn hoặc bằng 0.")]
        public decimal? MinimumBookingValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải lớn hơn hoặc bằng 0.")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn không được để trống.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(VoucherViewModel), nameof(ValidateExpiryDate))]
        public DateTime? ExpiryDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn hoặc bằng 0.")]
        public int? UsageLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sử dụng phải lớn hơn hoặc bằng 0.")]
        public int? UsageCount { get; set; }

        public bool IsActive { get; set; } = true;

        // Custom validation for ExpiryDate
        public static ValidationResult? ValidateExpiryDate(DateTime? expiryDate, ValidationContext context)
        {
            if (!expiryDate.HasValue)
                return new ValidationResult("Ngày hết hạn là bắt buộc.");

            if (expiryDate.Value.Date < DateTime.Now.Date)
                return new ValidationResult("Ngày hết hạn không được nhỏ hơn ngày hiện tại.");

            return ValidationResult.Success;
        }
    }
}
