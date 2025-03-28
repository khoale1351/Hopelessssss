using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class CustomerSupport
{
    public int TicketId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    [StringLength(255)]
    [RegularExpression("^(Booking|Payment|Technical|General|Others)$", ErrorMessage = "Invalid Issue Type.")]
    public string IssueType { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression("^(Open|In Progress|Resolved|Closed)$", ErrorMessage = "Invalid Status.")]
    public string Status { get; set; } = "Open";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
