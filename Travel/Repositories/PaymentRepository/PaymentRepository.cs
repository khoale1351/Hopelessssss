using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.PaymentRepository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private new readonly TourismDbContext _context;

        public PaymentRepository(TourismDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Payments.CountAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllAsync(Func<IQueryable<Payment>, IQueryable<Payment>> filter)
        {
            IQueryable<Payment> query = _context.Payments;
            if (filter != null)
            {
                query = filter(query);
            }
            return await query.ToListAsync();
        }
    }
}