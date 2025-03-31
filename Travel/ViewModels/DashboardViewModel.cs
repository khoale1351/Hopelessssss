using Travel.Models;
using System.Collections.Generic;

namespace Travel.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTours { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookings { get; set; }

        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public List<Tour> RecentTours { get; set; } = new List<Tour>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
    }
}