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
    { // Thêm dấu { ở đây
        private readonly TourismDbContext _context;

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

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string searchQuery)
        {
            // Kiểm tra nếu có searchQuery, lọc theo người dùng hoặc tour
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(b => b.User.FullName.Contains(searchQuery) || b.Tour.TourName.Contains(searchQuery));
            }

            return await query.ToListAsync();
        }

        public IQueryable<Booking> GetBookingsQueryable()
        {
            return _context.Bookings.Include(b => b.User).Include(b => b.Tour);
        }

    }
}