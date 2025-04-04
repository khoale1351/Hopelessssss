using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Travel.Data;
using Travel.Models;
using Travel.ViewModels;

namespace Travel.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly TourismDbContext _context;

        public BookingController(TourismDbContext context)
        {
            _context = context;
        }

        // GET: /Booking/Index
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var bookings = _context.Bookings
                .Include(b => b.Tour)
                .ThenInclude(t => t.Destination)
                .Include(b => b.Voucher)
                .Where(b => b.UserId == userId)
                .ToList();
            return View(bookings);
        }

        // GET: /Booking/Create?tourId=1
        public async Task<IActionResult> Create(int tourId)
        {
            var tour = await _context.Tours
                .Include(t => t.Destination)
                .FirstOrDefaultAsync(t => t.TourId == tourId);

            if (tour == null || tour.TourStatus != "Upcoming" || tour.AvailableSeats <= 0)
            {
                return NotFound();
            }

            // Lấy danh sách voucher có sẵn
            var availableVouchers = await _context.Vouchers
                .Where(v => v.ExpiryDate >= DateTime.UtcNow && v.IsActive)
                .Where(v => !v.UsageLimit.HasValue || !v.UsageCount.HasValue || v.UsageCount < v.UsageLimit)
                .ToListAsync();

            var model = new BookingViewModel
            {
                TourId = tour.TourId,
                TourName = tour.TourName,
                TourPrice = tour.Price,
                NumberOfAdults = 1, // Mặc định 1 người lớn
                NumberOfChildren = 0, // Mặc định 0 trẻ em
                AvailableVouchers = availableVouchers
            };

            return View(model);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            var tour = await _context.Tours.FindAsync(model.TourId);
            if (tour == null || tour.AvailableSeats < (model.NumberOfAdults + model.NumberOfChildren))
            {
                ModelState.AddModelError("", "Tour is not available or not enough seats.");
            }

            if (ModelState.IsValid)
            {
                // Tính tổng số người
                int totalPeople = model.NumberOfAdults + model.NumberOfChildren;

                // Tính tổng giá trước khi áp dụng giảm giá
                decimal totalPrice = tour.Price * totalPeople;

                // Áp dụng voucher nếu có
                if (model.VoucherID.HasValue)
                {
                    var voucher = await _context.Vouchers.FindAsync(model.VoucherID);
                    if (voucher != null && voucher.ExpiryDate >= DateTime.UtcNow && voucher.IsActive)
                    {
                        // Kiểm tra UsageLimit
                        if (voucher.UsageLimit.HasValue && voucher.UsageCount.HasValue && voucher.UsageCount >= voucher.UsageLimit)
                        {
                            ModelState.AddModelError("", "Voucher đã hết lượt sử dụng.");
                        }
                        // Kiểm tra MinimumBookingValue
                        else if (voucher.MinimumBookingValue.HasValue && totalPrice < voucher.MinimumBookingValue)
                        {
                            ModelState.AddModelError("", $"Tổng giá phải lớn hơn hoặc bằng {voucher.MinimumBookingValue:C} để sử dụng voucher này.");
                        }
                        else
                        {
                            if (voucher.DiscountPercentage.HasValue && voucher.DiscountPercentage > 0)
                            {
                                // Áp dụng giảm giá theo phần trăm
                                model.DiscountPercentageApplied = voucher.DiscountPercentage;
                                model.DiscountAmountApplied = totalPrice * (voucher.DiscountPercentage.Value / 100);

                                // Kiểm tra MaxDiscountAmount
                                if (voucher.MaxDiscountAmount.HasValue && model.DiscountAmountApplied > voucher.MaxDiscountAmount)
                                {
                                    model.DiscountAmountApplied = voucher.MaxDiscountAmount;
                                }

                                totalPrice -= model.DiscountAmountApplied.Value;
                            }
                            else
                            {
                                // Áp dụng giảm giá theo số tiền cố định
                                model.DiscountAmountApplied = voucher.DiscountAmount;
                                totalPrice -= voucher.DiscountAmount;
                                if (totalPrice < 0) totalPrice = 0; // Đảm bảo giá không âm
                            }

                            // Cập nhật UsageCount
                            voucher.UsageCount = (voucher.UsageCount ?? 0) + 1;
                            _context.Vouchers.Update(voucher);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    // Tạo booking
                    var booking = new Booking
                    {
                        TourId = model.TourId,
                        UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        NumberOfAdults = model.NumberOfAdults,
                        NumberOfChildren = model.NumberOfChildren,
                        TotalPrice = totalPrice,
                        BookingDate = DateTime.UtcNow,
                        Status = "Pending",
                        PaymentStatus = "Pending",
                        StartDate = DateOnly.FromDateTime(tour.StartDate), // Lấy từ tour
                        VoucherID = model.VoucherID,
                        DiscountAmountApplied = model.DiscountAmountApplied,
                        DiscountPercentageApplied = model.DiscountPercentageApplied
                    };

                    // Cập nhật số ghế còn lại
                    tour.AvailableSeats -= totalPeople;
                    _context.Tours.Update(tour);

                    // Lưu booking
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("BookingConfirmation", new { bookingId = booking.BookingId });
                }
            }

            // Nếu ModelState không hợp lệ, tải lại danh sách voucher
            model.AvailableVouchers = await _context.Vouchers
                .Where(v => v.ExpiryDate >= DateTime.UtcNow && v.IsActive)
                .Where(v => !v.UsageLimit.HasValue || !v.UsageCount.HasValue || v.UsageCount < v.UsageLimit)
                .ToListAsync();

            return View(model);
        }

        // GET: /Booking/Details/1
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .ThenInclude(t => t.Destination)
                .Include(b => b.Voucher)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: /Booking/Cancel/1
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Voucher)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Kiểm tra xem booking có thể hủy không (chỉ hủy nếu trạng thái là Pending)
            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Chỉ có thể hủy booking ở trạng thái Pending.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo bản ghi trong BookingLog
            var bookingLog = new BookingLog
            {
                BookingId = booking.BookingId,
                OldStatus = booking.Status, // "Pending"
                NewStatus = "Cancelled",
                ChangedBy = booking.UserId, // Lấy UserId của người đặt booking
                ChangedAt = DateTime.UtcNow
            };
            _context.BookingLogs.Add(bookingLog);

            // Hoàn lại số ghế
            if (booking.Tour != null)
            {
                booking.Tour.AvailableSeats += (booking.NumberOfAdults + booking.NumberOfChildren);
                _context.Tours.Update(booking.Tour);
            }

            // Giảm UsageCount của voucher nếu có
            if (booking.VoucherID.HasValue && booking.Voucher != null)
            {
                booking.Voucher.UsageCount = (booking.Voucher.UsageCount ?? 0) - 1;
                if (booking.Voucher.UsageCount < 0) booking.Voucher.UsageCount = 0;
                _context.Vouchers.Update(booking.Voucher);
            }

            // Xóa bản ghi Booking
            _context.Bookings.Remove(booking);

            // Lưu tất cả thay đổi
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/BookingConfirmation?bookingId=1
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .ThenInclude(t => t.Destination)
                .Include(b => b.Voucher)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}