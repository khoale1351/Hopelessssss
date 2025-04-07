using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ToursRepository
{
    public class TourRepository : GenericRepository<Tour>, ITourRepository
    {
        private new readonly TourismDbContext _context;

        public TourRepository(TourismDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Tours.CountAsync();
        }

        public async Task<IEnumerable<Tour>> GetAllAsync(Func<IQueryable<Tour>, IQueryable<Tour>> filter)
        {
            IQueryable<Tour> query = _context.Tours;
            if (filter != null)
            {
                query = filter(query);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetToursByDestinationIdAsync(int destinationId)
        {
            return await _context.Tours.Where(t => t.DestinationId == destinationId).ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetUpcomingToursAsync()
        {
            return await _context.Tours.Where(t => t.StartDate > DateTime.Now).ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetOngoingToursAsync()
        {
            return await _context.Tours
                .Where(t => t.StartDate <= DateTime.Now && t.EndDate >= DateTime.Now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetCompletedToursAsync()
        {
            return await _context.Tours.Where(t => t.TourStatus == "Completed").ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetCancelledToursAsync()
        {
            return await _context.Tours.Where(t => t.TourStatus == "Cancelled").ToListAsync();
        }
    }
}