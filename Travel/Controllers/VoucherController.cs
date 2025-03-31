using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel.Models;
using Travel.Repositories;

namespace Travel.Controllers
{
    [Authorize]
    public class VoucherController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VoucherController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Cho phép Admin, Agent, Guide, Employee, Vendor, Customer truy cập danh sách voucher
        [Authorize(Roles = "Admin,Agent,Employee")]
        public async Task<IActionResult> Index()
        {
            var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
            return View(vouchers);
        }

        // Tương tự, tất cả các role trên có thể xem chi tiết voucher
        [Authorize(Roles = "Admin,Agent,Employee")]
        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // Chỉ Admin mới có quyền tạo voucher
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VoucherId,Code,Description,DiscountAmount,DiscountPercentage,MinimumBookingValue,MaxDiscountAmount,ExpiryDate,UsageLimit,UsageCount,IsActive")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Vouchers.AddAsync(voucher);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // Chỉ Admin và Agent mới có quyền chỉnh sửa voucher
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Agent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VoucherId,Code,Description,DiscountAmount,DiscountPercentage,MinimumBookingValue,MaxDiscountAmount,ExpiryDate,UsageLimit,UsageCount,IsActive")] Voucher voucher)
        {
            if (id != voucher.VoucherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.Vouchers.UpdateAsync(voucher);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _unitOfWork.Vouchers.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // Chỉ Admin có quyền xóa voucher
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher != null)
            {
                await _unitOfWork.Vouchers.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Customer, Agent, Guide, Employee, Vendor có thể áp dụng voucher
        [Authorize(Roles = "Customer,Agent,Guide,Employee,Vendor")]
        public async Task<IActionResult> ApplyVoucher(int bookingId, string voucherCode)
        {
            try
            {
                await _unitOfWork.Vouchers.ApplyVoucherAsync(bookingId, voucherCode);
                return RedirectToAction("Index", "Bookings");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        // Lấy danh sách voucher đang hoạt động (cho tất cả các role)
        [Authorize(Roles = "Admin,Agent,Guide,Employee,Vendor,Customer")]
        public async Task<IActionResult> ActiveVouchers()
        {
            var activeVouchers = await _unitOfWork.Vouchers.GetActiveVouchersAsync();
            return View(activeVouchers);
        }

        // Lấy danh sách voucher đã sử dụng (chỉ Admin và Agent có thể xem)
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> UsedVouchers()
        {
            var usedVouchers = await _unitOfWork.Vouchers.GetUsedVouchersAsync();
            return View(usedVouchers);
        }

        // Lấy danh sách voucher chưa sử dụng nhưng vẫn còn hạn (cho tất cả role)
        [Authorize(Roles = "Admin,Agent,Guide,Employee,Vendor,Customer")]
        public async Task<IActionResult> UnusedActiveVouchers()
        {
            var unusedActiveVouchers = await _unitOfWork.Vouchers.GetUnusedActiveVouchersAsync();
            return View(unusedActiveVouchers);
        }
    }
}
