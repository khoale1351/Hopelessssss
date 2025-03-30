using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount amount must be greater than or equal 0.")]
    public decimal DiscountAmount { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100.")]
    public decimal? DiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Minimum booking value must be greater than or equal 0.")]
    public decimal? MinimumBookingValue { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Maximum discount amount must be greater than or equal 0.")]
    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Usage limit must be greater than or equal 0.")]
    public int? UsageLimit { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Usage count must be greater than or equal 0.")]
    public int? UsageCount { get; set; }

    public bool IsActive { get; set; } = true;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
