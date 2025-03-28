using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories.PublicRepository;

namespace Travel.Repositories.Vouchers
{
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(TourismDbContext context) : base(context)
        {
        }

        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == code && v.IsActive && v.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            return await _context.Vouchers
                .Where(v => v.IsActive && v.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        // Thay đổi parameter userId thành string
        public async Task<IEnumerable<Voucher>> GetVouchersByUserAsync(string userId)
        {
            return await _context.Vouchers
                .Where(v => _context.Bookings.Any(b => b.UserId == userId && b.VoucherID == v.VoucherId))
                .ToListAsync();
        }

        public async Task<bool> IsVoucherValidAsync(string voucherCode, decimal totalPrice)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(
                v => v.Code == voucherCode && v.IsActive && v.ExpiryDate > DateTime.UtcNow);
            if (voucher == null)
                return false;

            // Kiểm tra giá trị tối thiểu
            if (voucher.MinimumBookingValue.HasValue && totalPrice < voucher.MinimumBookingValue.Value)
                return false;

            // Kiểm tra số lần sử dụng (nếu có)
            if (voucher.UsageLimit.HasValue)
            {
                int usedCount = await _context.Bookings
                    .CountAsync(b => b.VoucherID == voucher.VoucherId);
                if (usedCount >= voucher.UsageLimit.Value)
                    return false;
            }

            return true;
        }

        public async Task ApplyVoucherAsync(int bookingId, string voucherCode)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.VoucherID.HasValue)
                throw new Exception("Voucher has already been applied to this booking");

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == voucherCode && v.IsActive && v.ExpiryDate > DateTime.UtcNow);
            if (voucher == null)
                throw new Exception("Invalid or expired voucher");

            // Kiểm tra số lần sử dụng
            if (voucher.UsageLimit.HasValue)
            {
                int usedCount = await _context.Bookings
                    .CountAsync(b => b.VoucherID == voucher.VoucherId);
                if (usedCount >= voucher.UsageLimit.Value)
                    throw new Exception("Voucher usage limit exceeded");
            }

            booking.VoucherID = voucher.VoucherId;

            if (voucher.DiscountAmount > 0)
                booking.DiscountAmountApplied = voucher.DiscountAmount;
            else if (voucher.DiscountPercentage > 0)
                booking.DiscountPercentageApplied = voucher.DiscountPercentage;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveVoucherFromBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new Exception("Booking not found");

            if (!booking.VoucherID.HasValue)
                throw new Exception("No voucher applied to this booking");

            booking.VoucherID = null;
            booking.DiscountAmountApplied = 0;
            booking.DiscountPercentageApplied = 0;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Voucher>> GetUsedVouchersAsync()
        {
            return await _context.Vouchers
                .Where(v => _context.Bookings.Any(b => b.VoucherID == v.VoucherId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Voucher>> GetUnusedActiveVouchersAsync()
        {
            return await _context.Vouchers
                .Where(v => v.IsActive && v.ExpiryDate > DateTime.UtcNow
                    && !_context.Bookings.Any(b => b.VoucherID == v.VoucherId))
                .ToListAsync();
        }
    }
}
