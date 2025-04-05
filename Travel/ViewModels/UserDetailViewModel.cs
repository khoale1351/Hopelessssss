namespace Travel.ViewModels
{
    public class UserDetailViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; }
        public string MembershipType { get; set; }
        public string Status { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
