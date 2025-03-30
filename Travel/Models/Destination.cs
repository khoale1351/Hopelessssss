using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models;

public partial class Destination
{
    public int DestinationId { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    public bool IsPopular { get; set; } = false;

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
