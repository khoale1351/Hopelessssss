namespace Travel.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class UserDetailViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; }
        public string MembershipType { get; set; }
        public string Status { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
