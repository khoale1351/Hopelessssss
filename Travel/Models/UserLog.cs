using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class UserLog
{
    public int LogId { get; set; }

    public string? UserId { get; set; }

    [StringLength(255)]
    public string? OldEmail { get; set; }

    [StringLength(255)]
    public string? NewEmail { get; set; }

    [StringLength(20)]
    public string? OldPhone { get; set; }

    [StringLength(20)]
    public string? NewPhone { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ChangedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}
