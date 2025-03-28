using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.BookingsRepository
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepostiory
    {
        public BookingRepository(TourismDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userEmail)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Where(b => b.User.Email == userEmail).ToListAsync();
        }
    }
}
