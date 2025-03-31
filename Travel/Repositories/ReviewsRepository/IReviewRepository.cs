using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ReviewsRepository
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<int> CountAsync();
        Task<IEnumerable<Review>> GetAllAsync(Func<IQueryable<Review>, IQueryable<Review>> filter); // Sửa kiểu filter
        Task<IEnumerable<Review>> GetReviewsByTourIdAsync(int tourId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    }
}