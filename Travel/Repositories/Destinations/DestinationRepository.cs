using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;


namespace Travel.Repositories.Destinations
{
    public class DestinationRepository : GenericRepository<Destination>, IDestinationRepository
    {
        public DestinationRepository(TourismDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Destination>> GetPopularDestinationsAsync()
        {
            return await _context.Destinations.Where(d => d.IsPopular == true).ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetDestinationsByCountryAsync(string country)
        {
            return await _context.Destinations
                .Where(d => d.Country.ToLower() == country.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetDestinationsByCityAsync(string city)
        {
            return await _context.Destinations
                .Where(d => d.City.ToLower() == city.ToLower())
                .ToListAsync();
        }
    }
}
