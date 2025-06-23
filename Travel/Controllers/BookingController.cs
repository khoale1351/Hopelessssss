using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if user already has a booking for this tour
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == userId && b.TourId == tourId);

            if (existingBooking != null)
            {
                TempData["Error"] = "Bạn đã đặt tour này rồi.";
                return RedirectToAction("Index");
            }

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
                .Where(v => !v.UsageLimit.HasValue || v.UsageCount < v.UsageLimit)
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check if user already has a booking for this tour
                var existingBooking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.TourId == model.TourId);

                if (existingBooking != null)
                {
                    ModelState.AddModelError("", "Bạn đã đặt tour này rồi.");
                }

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
                        if (voucher.UsageLimit.HasValue && voucher.UsageCount >= voucher.UsageLimit)
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
                .Where(v => !v.UsageLimit.HasValue || v.UsageCount < v.UsageLimit)
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Tours = new SelectList(await _context.Tours.ToListAsync(), "TourId", "TourName", booking.TourId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, booking.Status);
            ViewBag.PaymentStatusList = new SelectList(new[] { "Pending", "Paid", "Refunded" }, booking.PaymentStatus);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,UserId,TourId,NumberOfAdults,NumberOfChildren,BookingDate,Status,PaymentStatus,VoucherID,StartDate,DiscountAmountApplied,DiscountPercentageApplied")] Booking booking, string searchQuery, string statusFilter)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin booking hiện tại từ cơ sở dữ liệu
                var existingBooking = await _context.Bookings
                    .Include(b => b.Tour)
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                if (existingBooking == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường có thể chỉnh sửa
                existingBooking.Status = booking.Status;
                existingBooking.PaymentStatus = booking.PaymentStatus;
                existingBooking.StartDate = booking.StartDate;
                existingBooking.DiscountAmountApplied = booking.DiscountAmountApplied;
                existingBooking.DiscountPercentageApplied = booking.DiscountPercentageApplied;

                // Tính toán lại TotalPrice
                if (existingBooking.Tour != null)
                {
                    decimal basePrice = existingBooking.Tour.Price * (existingBooking.NumberOfAdults + existingBooking.NumberOfChildren);
                    decimal discountAmount = booking.DiscountAmountApplied ?? 0;
                    decimal discountPercentage = booking.DiscountPercentageApplied ?? 0;
                    decimal discountFromPercentage = basePrice * (discountPercentage / 100);
                    existingBooking.TotalPrice = basePrice - discountAmount - discountFromPercentage;
                }

                // Cập nhật booking trong database
                _context.Update(existingBooking);
                await _context.SaveChangesAsync();

                // Sau khi lưu thành công, chuyển hướng về ManageBookings
                return RedirectToAction("ManageBookings", "Admin", new { searchQuery = searchQuery, statusFilter = statusFilter });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lưu thay đổi: {ex.Message}");
            }

            // Nếu model không hợp lệ, truyền lại ViewBag và quay lại trang Edit
            ViewBag.Tours = new SelectList(await _context.Tours.ToListAsync(), "TourId", "TourName", booking.TourId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, booking.Status);
            ViewBag.PaymentStatusList = new SelectList(new[] { "Pending", "Paid", "Refunded" }, booking.PaymentStatus);

            // Trả lại view với model lỗi
            return View(booking);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageBookings", "Admin");
        }

        public async Task<IActionResult> BookingDetails(int id)
        {
            // Lấy thông tin Booking theo BookingId
            var booking = await _context.Bookings
                .Include(b => b.User) // Lấy thông tin User
                .Include(b => b.Tour) // Lấy thông tin Tour
                .Include(b => b.Voucher) // Lấy thông tin Voucher
                .Include(b => b.BookingLogs) // Lấy lịch sử thay đổi trạng thái
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}