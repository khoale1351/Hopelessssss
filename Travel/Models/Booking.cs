using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public string UserId { get; set; }

    public int? TourId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Number of adults must be greater than or equal 0")]
    public int NumberOfAdults { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Number of children must be greater than or equal 0")]
    public int NumberOfChildren { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Total price must be greater than or equal 0")]
    public decimal TotalPrice { get; set; }

    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    [Required]
    [RegularExpression("^(Pending|Confirmed|Cancelled|Completed)$", ErrorMessage = "Invalid Status.")]
    public string Status { get; set; } = "Pending";

    [RegularExpression("^(Pending|Paid|Refunded)$", ErrorMessage = "Invalid Payment Status.")]
    public string? PaymentStatus { get; set; } = "Pending";

    public int? VoucherID { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Discount amount applied must be greater than or equal 0")]
    public decimal? DiscountAmountApplied { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percentage applied must be between 0 and 100")]
    public decimal? DiscountPercentageApplied { get; set; }

    public virtual ICollection<BookingLog> BookingLogs { get; set; } = new List<BookingLog>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    
    [ForeignKey("VoucherID")]
    public virtual Voucher? Voucher { get; set; }

    public virtual Tour? Tour { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
