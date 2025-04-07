using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.BookingsRepository
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<int> CountAsync();
        Task<IEnumerable<Booking>> GetAllAsync(Func<IQueryable<Booking>, IQueryable<Booking>> filter);
        Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userEmail);
        Task<IEnumerable<Booking>> GetBookingsAsync(string searchQuery);
        IQueryable<Booking> GetBookingsQueryable();
    }
}