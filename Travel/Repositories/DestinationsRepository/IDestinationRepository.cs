using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.DestinationsRepository
{
    public interface IDestinationRepository : IGenericRepository<Destination>
    {
        Task<IEnumerable<Destination>> GetPopularDestinationsAsync();
        Task<IEnumerable<Destination>> GetDestinationsByCountryAsync(string country);
        Task<IEnumerable<Destination>> GetDestinationsByCityAsync(string city);
    }
}
