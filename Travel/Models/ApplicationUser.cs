using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Travel.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Required, StringLength(50)]
        public string MembershipType { get; set; } = "Silver";

        [Required, StringLength(50)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<BookingLog> BookingLogs { get; set; } = new List<BookingLog>();
        public virtual ICollection<CustomerSupport> CustomerSupports { get; set; } = new List<CustomerSupport>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
        public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
        public virtual ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
    }
}
