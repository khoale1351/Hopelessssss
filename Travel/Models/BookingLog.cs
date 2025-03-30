using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class BookingLog
{
    public int LogId { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    [StringLength(50)]
    public string OldStatus { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string NewStatus { get; set; } = null!;

    public string? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("BookingId")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("ChangedBy")]
    public virtual ApplicationUser? ChangedByNavigation { get; set; }
}
