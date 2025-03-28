using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class TransactionHistory
{
    public int TransactionId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression("^(Booking|Refund|Membership Payment)$", ErrorMessage = "Invalid Transaction Type.")]
    public string TransactionType { get; set; } = null!;

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
