using Microsoft.EntityFrameworkCore;
using Travel.Data;

namespace Travel.Repositories.PublicRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly TourismDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(TourismDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _dbSet.Remove(entity);
            return true;
        }
    }
}
