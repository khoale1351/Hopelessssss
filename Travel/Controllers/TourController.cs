using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Travel.Data;
using Travel.Models;
using Travel.Repositories;
using Travel.ViewModels;
using Microsoft.EntityFrameworkCore;
using Travel.Repositories.IMAGESERVICE;
using SixLabors.ImageSharp;

public class TourController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TourismDbContext _context;
    private readonly IImageService _imageService;

    public TourController(IUnitOfWork unitOfWork, TourismDbContext context, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _imageService = imageService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var tours = await _unitOfWork.Tours.GetAllAsync();
        var tourViewModels = tours.Select(t => new TourViewModel
        {
            TourId = t.TourId,
            TourName = t.TourName,
            DestinationName = t.Destination?.Name ?? "Không có điểm đến",
            Price = t.Price,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            AvailableSeats = t.AvailableSeats,
            TourType = t.TourType,
            TourStatus = t.TourStatus
        }).ToList();

        return View(tourViewModels);
    }

    [HttpGet]
    public async Task<IActionResult> SearchDestinations(string searchTerm)
    {
        try
        {
            var destinations = await _unitOfWork.Destinations.GetAllAsync();

            var results = destinations
                .Where(d => string.IsNullOrEmpty(searchTerm) ||
                            d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            (d.City != null && d.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (d.Country != null && d.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .Select(d => new
                {
                    destinationId = d.DestinationId,
                    name = d.Name,
                    city = d.City,
                    country = d.Country
                })
                .Take(50)
                .ToList();

            return Json(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Lỗi khi tải điểm đến: " + ex.Message });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TourViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TourViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var newTour = new Tour
                {
                    TourName = model.TourName,
                    Description = model.Description,
                    Price = model.Price,
                    Duration = model.Duration,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    AvailableSeats = model.AvailableSeats,
                    TourType = model.TourType,
                    TourStatus = model.TourStatus,
                    DestinationId = model.DestinationId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Tours.AddAsync(newTour);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tour created successfully!";
                return RedirectToAction("ManageTours", "Admin");
            }

            // Nếu có lỗi validation, load lại danh sách điểm đến
            model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                .Select(d => new SelectListItem
                {
                    Value = d.DestinationId.ToString(),
                    Text = d.Name
                }).ToList();
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error creating tour: " + ex.Message);
            model.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                .Select(d => new SelectListItem
                {
                    Value = d.DestinationId.ToString(),
                    Text = d.Name
                }).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTourDetails(int id)
    {
        Console.WriteLine($"Đang gọi GetTourDetails với id: {id}");
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour == null) return NotFound();

        // Load thông tin liên quan nếu cần
        tour.Destination = await _unitOfWork.Destinations.GetByIdAsync(tour.DestinationId);

        return PartialView("_TourDetailPartial", tour);
    }

    [HttpGet]
    public async Task<IActionResult> GetEditTourForm(int id)
    {
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour == null) return NotFound();

        // Load danh sách điểm đến cho dropdown
        ViewBag.Destinations = await _unitOfWork.Destinations.GetAllAsync();

        return PartialView("_EditTourPartial", tour);
    }

    [HttpPost]
    public async Task<IActionResult> EditTour(
        int TourId, string TourName, int DestinationId, string Description, decimal Price, int Duration,
        DateTime StartDate, DateTime EndDate,
        int AvailableSeats, string TourType, string TourStatus, IFormFile ImageFile)
    {
        try
        {
            var existingTour = await _unitOfWork.Tours.GetByIdAsync(TourId);
            if (existingTour == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin tour
            existingTour.TourName = TourName;
            existingTour.DestinationId = DestinationId;
            existingTour.Description = Description;
            existingTour.Price = Price;
            existingTour.Duration = Duration;
            existingTour.StartDate = StartDate;
            existingTour.EndDate = EndDate;
            existingTour.AvailableSeats = AvailableSeats;
            existingTour.TourType = TourType;
            existingTour.TourStatus = TourStatus;

            // Xử lý hình ảnh (nếu có)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var tourId = existingTour.TourId;

                var uniqueFileName = $"tour-{tourId}";
                // Gọi ImageService để lưu ảnh
                var result = await _imageService.SaveImageAsync(
                    ImageFile,
                    "images/tours",
                    filePrefix: uniqueFileName,
                    oldFilePath: existingTour.ImageUrl,
                    targetSize: new Size(800, 600)
                );

                if (result.IsSuccess)
                {
                    existingTour.ImageUrl = result.FilePath;
                }
                else
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View();
                }
            }

            // Cập nhật dữ liệu tour
            await _unitOfWork.Tours.UpdateAsync(existingTour);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { redirect = Url.Action("ManageTours", "Admin") });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi khi cập nhật tour: " + ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTour(int id)
    {
        try
        {
            // Kiểm tra xem tour có tồn tại không
            var tour = await _unitOfWork.Tours.GetByIdAsync(id);
            if (tour == null)
            {
                return Json(new { success = false, message = "Tour không tồn tại." });
            }

            // Xóa các booking liên quan (nếu có)
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var relatedBookings = bookings.Where(b => b.TourId == id).ToList();
            foreach (var booking in relatedBookings)
            {
                await _unitOfWork.Bookings.DeleteAsync(booking.BookingId);
            }

            // Xóa các đánh giá liên quan (nếu có)
            var reviews = await _unitOfWork.Reviews.GetAllAsync();
            var relatedReviews = reviews.Where(r => r.TourId == id).ToList();
            foreach (var review in relatedReviews)
            {
                await _unitOfWork.Reviews.DeleteAsync(review.ReviewId);
            }

            // Xóa tour
            await _unitOfWork.Tours.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa tour thành công!", redirect = Url.Action("ManageTours", "Admin") });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi xóa tour: " + ex.Message });
        }
    }
}
