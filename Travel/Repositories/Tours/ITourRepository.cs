using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ToursRepository
{
    public interface ITourRepository : IGenericRepository<Tour>
    {
        Task<IEnumerable<Tour>> GetToursByDestinationIdAsync(int DestinationId);
        Task<IEnumerable<Tour>> GetUpcomingToursAsync();
        Task<IEnumerable<Tour>> GetOngoingToursAsync();
        Task<IEnumerable<Tour>> GetCompletedToursAsync();
        Task<IEnumerable<Tour>> GetCancelledToursAsync();
    }
}
