using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.PaymentRepository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(TourismDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByUserAsync(string userEmail)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                .Where(p => p.User.Email == userEmail).ToListAsync();
        }
    }
}
