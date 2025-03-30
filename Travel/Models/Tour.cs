using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class Tour
{
    public int TourId { get; set; }

    [Required]
    public int DestinationId { get; set; }

    [Required, StringLength(255)]
    public string TourName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than or equal 1")]
    public int Duration { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Available seats must be greater than or equal 0")]
    public int AvailableSeats { get; set; }

    public string? TourGuideId { get; set; }

    public string? ImageUrl { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required, StringLength(50)]
    [RegularExpression("^(Private|Group)$", ErrorMessage = "Tour type must be either Private or Group")]
    public string TourType { get; set; } = "Private";

    [Required, StringLength(50)]
    [RegularExpression("^(Upcoming|Ongoing|Completed|Cancelled)$", ErrorMessage = "Invalid TourStatus.")]
    public string TourStatus { get; set; } = "Upcoming";

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Destination Destination { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ApplicationUser? TourGuide { get; set; }
}
