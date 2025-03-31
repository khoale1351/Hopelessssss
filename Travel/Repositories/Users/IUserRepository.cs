using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.Users
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetByPhoneNumberAsync(string phoneNumber);
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<ApplicationUser>> GetActiveUserAsync();
        Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync();
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<int> CountAsync();
        Task<IEnumerable<ApplicationUser>> GetAllAsync(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> filter);
    }
}