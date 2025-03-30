using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.Destinations
{
    public interface IDestinationRepository : IGenericRepository<Destination>
    {
        Task<IEnumerable<Destination>> GetPopularDestinationsAsync();
        Task<IEnumerable<Destination>> GetDestinationsByCountryAsync(string country);
        Task<IEnumerable<Destination>> GetDestinationsByCityAsync(string city);
    }
}
