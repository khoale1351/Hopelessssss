using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.ReviewsRepository
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private new readonly TourismDbContext _context;

        public ReviewRepository(TourismDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Reviews.CountAsync();
        }

        public async Task<IEnumerable<Review>> GetAllAsync(Func<IQueryable<Review>, IQueryable<Review>> filter)
        {
            IQueryable<Review> query = _context.Reviews;
            if (filter != null)
            {
                query = filter(query);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByTourIdAsync(int tourId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TourId == tourId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}