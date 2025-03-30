using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    [RegularExpression("^(Credit Card|PayPal|Bank Transfer|Cash|Crypto)$", ErrorMessage = "Invalid Payment Method.")]
    public string PaymentMethod { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression("^(Pending|Paid|Refunded)$", ErrorMessage = "Invalid Payment Status.")]
    public string PaymentStatus { get; set; } = "Pending";

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
