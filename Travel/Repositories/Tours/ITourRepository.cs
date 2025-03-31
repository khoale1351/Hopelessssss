using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        Task<int> CountAsync();
        Task<IEnumerable<Tour>> GetAllAsync(Func<IQueryable<Tour>, IQueryable<Tour>> filter); // Sửa kiểu filter
    }
}