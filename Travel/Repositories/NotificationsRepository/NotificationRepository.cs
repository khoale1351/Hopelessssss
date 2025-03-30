using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.NotificationsRepository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(TourismDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        }
    }
}
