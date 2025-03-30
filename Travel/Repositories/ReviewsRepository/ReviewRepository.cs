using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ReviewsRepository
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(TourismDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetReviewsByTourIdAsync(int tourId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TourId == tourId).ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.Reviews.Where(r => r.UserId == userId).ToListAsync();
        }
    }
}
