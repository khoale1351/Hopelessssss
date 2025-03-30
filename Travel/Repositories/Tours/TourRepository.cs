using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ToursRepository
{
    public class TourRepository : GenericRepository<Tour>, ITourRepository
    {
        public TourRepository(TourismDbContext context) : base(context)
        {
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
