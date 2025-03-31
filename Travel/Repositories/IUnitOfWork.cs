using Travel.Repositories.BookingsRepository;
using Travel.Repositories.DestinationsRepository;
using Travel.Repositories.NotificationsRepository;
using Travel.Repositories.PaymentRepository;
using Travel.Repositories.ReviewsRepository;
using Travel.Repositories.ToursRepository;
using Travel.Repositories.Users;
using Travel.Repositories.Vouchers;

namespace Travel.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IDestinationRepository Destinations { get; }
        ITourRepository Tours { get; }
        IVoucherRepository Vouchers { get; }
        IBookingRepostiory Bookings { get; }
        IPaymentRepository Payments { get; }
        IReviewRepository Reviews { get; }
        INotificationRepository Notifications { get; }
        Task<int> SaveChangesAsync();
    }
}
