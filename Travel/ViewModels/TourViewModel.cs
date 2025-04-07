// ViewModels/TourViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Travel.ViewModels
{
    public class TourViewModel
    {
        public int TourId { get; set; }
        public int DestinationId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableSeats { get; set; }
        public string TourType { get; set; } = string.Empty;
        public string TourStatus { get; set; } = string.Empty;
        public int Duration { get; set; }
        public IEnumerable<SelectListItem>? DestinationOptions { get; set; }

        public string? ImageUrl { get; set; }
    }
}