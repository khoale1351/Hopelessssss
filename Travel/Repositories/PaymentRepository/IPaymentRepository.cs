using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.PaymentRepository
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<int> CountAsync();
        Task<IEnumerable<Payment>> GetAllAsync(Func<IQueryable<Payment>, IQueryable<Payment>> filter);
    }
}