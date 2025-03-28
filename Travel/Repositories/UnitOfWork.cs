using Microsoft.AspNetCore.Identity;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.BookingsRepository;
using Travel.Repositories.Destinations;
using Travel.Repositories.NotificationsRepository;
using Travel.Repositories.PaymentRepository;
using Travel.Repositories.ReviewsRepository;
using Travel.Repositories.ToursRepository;
using Travel.Repositories.Users;
using Travel.Repositories.Vouchers;

namespace Travel.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TourismDbContext _context;
        public IUserRepository Users { get; private set; }
        public IDestinationRepository Destinations { get; private set; }
        public ITourRepository Tours { get; private set; }
        public IVoucherRepository Vouchers { get; private set; }
        public IBookingRepostiory Bookings { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public INotificationRepository Notifications { get; private set; }

        public UnitOfWork(TourismDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            Users = new UserRepository(_context, userManager);
            Destinations = new DestinationRepository(_context);
            Tours = new TourRepository(_context);
            Vouchers = new VoucherRepository(_context);
            Bookings = new BookingRepository(_context);
            Payments = new PaymentRepository.PaymentRepository(_context);
            Reviews = new ReviewRepository(_context);
            Notifications = new NotificationRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
