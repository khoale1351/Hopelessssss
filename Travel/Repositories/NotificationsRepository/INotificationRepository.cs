using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.NotificationsRepository
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId);
    }
}
