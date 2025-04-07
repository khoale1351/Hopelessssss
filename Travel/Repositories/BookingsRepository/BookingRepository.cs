using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.BookingsRepository
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private new readonly TourismDbContext _context;

        public BookingRepository(TourismDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Bookings.CountAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllAsync(Func<IQueryable<Booking>, IQueryable<Booking>> filter)
        {
            IQueryable<Booking> query = _context.Bookings;
            if (filter != null)
            {
                query = filter(query);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userEmail)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Where(b => b.User.Email == userEmail)
                .ToListAsync();
        }
    }
}