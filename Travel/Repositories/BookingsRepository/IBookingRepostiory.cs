using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.BookingsRepository
{
    public interface IBookingRepostiory : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userEmail);
    }
}
