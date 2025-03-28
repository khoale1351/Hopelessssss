using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.Users
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(TourismDbContext context, UserManager<ApplicationUser> userManager)
            : base(context)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email không được để trống", nameof(email));
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.Trim() == email.Trim());
        }

        public async Task<ApplicationUser> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber.Trim() == phoneNumber.Trim());
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            // Lấy toàn bộ người dùng và lọc ra những người thuộc role yêu cầu
            var allUsers = await _context.Users.ToListAsync();
            var usersInRole = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    usersInRole.Add(user);
                }
            }

            return usersInRole;
        }

        public async Task<IEnumerable<ApplicationUser>> GetActiveUserAsync()
        {
            return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync()
        {
            return await _context.Users.Where(u => !u.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId.ToString());
            if (user == null) return false;

            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
