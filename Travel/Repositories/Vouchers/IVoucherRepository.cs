using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.Vouchers
{
    public interface IVoucherRepository : IGenericRepository<Voucher>
    {
        Task<Voucher> GetVoucherByCodeAsync(string voucherCode);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<IEnumerable<Voucher>> GetVouchersByUserAsync(string userId);
        Task<bool> IsVoucherValidAsync(string voucherCode, decimal totalPrice);
        Task ApplyVoucherAsync(int bookingId, string voucherCode);
        Task RemoveVoucherFromBookingAsync(int bookingId);
        Task<IEnumerable<Voucher>> GetUsedVouchersAsync();
        Task<IEnumerable<Voucher>> GetUnusedActiveVouchersAsync();
    }
}
