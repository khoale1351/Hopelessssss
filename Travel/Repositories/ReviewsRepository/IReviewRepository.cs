using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ReviewsRepository
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByTourIdAsync(int tourId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    }
}
