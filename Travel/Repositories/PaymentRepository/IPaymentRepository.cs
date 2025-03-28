using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.PaymentRepository
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(string userEmail);
    }
}
