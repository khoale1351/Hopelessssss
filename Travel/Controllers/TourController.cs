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
                .Select(d => new {
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
            return StatusCode(500, new { error = ex.Message });
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
    public async Task<IActionResult> Edit(int id)
    {
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour == null)
        {
            return NotFound();
        }

        ViewBag.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
            .Select(d => new SelectListItem
            {
                Value = d.DestinationId.ToString(),
                Text = $"{d.Name} - {d.City}, {d.Country}"
            }).ToList();

        return View("EditTours", tour); // Chỉ định rõ view là EditTours.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tour model, IFormFile ImageFile)
    {
        Console.WriteLine($"TourId: {model.TourId}, TourName: {model.TourName}, ImageFile: {(ImageFile != null ? ImageFile.FileName : "null")}");
        try
        {
            if (id != model.TourId)
            {
                return NotFound();
            }

            // Kiểm tra ràng buộc phía server
            if (model.StartDate >= model.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày về phải sau ngày đi.");
            }

            var dayDiff = (model.EndDate - model.StartDate).Days + 1;
            if (dayDiff > 30)
            {
                ModelState.AddModelError("EndDate", "Tour không được vượt quá 30 ngày.");
            }

            if (dayDiff != model.Duration)
            {
                ModelState.AddModelError("Duration", $"Thời gian tour không khớp với ngày đi và ngày về. Dự kiến: {dayDiff} ngày");
            }

            if (model.AvailableSeats < 0)
            {
                ModelState.AddModelError("AvailableSeats", "Số chỗ không được nhỏ hơn 0.");
            }

            if (ModelState.IsValid)
            {
                var existingTour = await _unitOfWork.Tours.GetByIdAsync(id);
                if (existingTour == null)
                {
                    return NotFound();
                }

                existingTour.TourName = model.TourName;
                existingTour.DestinationId = model.DestinationId;
                existingTour.Description = model.Description;
                existingTour.Price = model.Price;
                existingTour.Duration = model.Duration;
                existingTour.StartDate = model.StartDate;
                existingTour.EndDate = model.EndDate;
                existingTour.AvailableSeats = model.AvailableSeats;
                existingTour.TourType = model.TourType;
                existingTour.TourStatus = model.TourStatus;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uniqueFileName = $"tour-{id}-{Guid.NewGuid().ToString()[..8]}";
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước ảnh không được vượt quá 5MB");
                        ViewBag.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                            .Select(d => new SelectListItem
                            {
                                Value = d.DestinationId.ToString(),
                                Text = $"{d.Name} - {d.City}, {d.Country}"
                            }).ToList();
                        return View("EditTours", model);
                    }

                    var result = await _imageService.SaveImageAsync(
                        ImageFile,
                        "images/tours",
                        filePrefix: uniqueFileName,
                        oldFilePath: existingTour.ImageUrl,
                        targetSize: new Size(800, 600)
                    );

                    if (!result.IsSuccess)
                    {
                        ModelState.AddModelError("ImageFile", result.ErrorMessage ?? "Unknown error occurred while saving the image.");
                        ViewBag.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                            .Select(d => new SelectListItem
                            {
                                Value = d.DestinationId.ToString(),
                                Text = $"{d.Name} - {d.City}, {d.Country}"
                            }).ToList();
                        return View("EditTours", model);
                    }

                    existingTour.ImageUrl = result.FilePath;
                }

                await _unitOfWork.Tours.UpdateAsync(existingTour);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật tour thành công!";
                return RedirectToAction("ManageTours", "Admin");
            }

            // Nếu ModelState không hợp lệ, trả về view EditTours với thông tin lỗi
            ViewBag.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                .Select(d => new SelectListItem
                {
                    Value = d.DestinationId.ToString(),
                    Text = $"{d.Name} - {d.City}, {d.Country}"
                }).ToList();

            return View("EditTours", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi khi cập nhật tour: {ex.Message}");
            ViewBag.DestinationOptions = (await _unitOfWork.Destinations.GetAllAsync())
                .Select(d => new SelectListItem
                {
                    Value = d.DestinationId.ToString(),
                    Text = $"{d.Name} - {d.City}, {d.Country}"
                }).ToList();
            return View("EditTours", model);
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var tour = await _context.Tours
            .Include(t => t.Destination)
            .FirstOrDefaultAsync(t => t.TourId == id);

        if (tour == null)
        {
            return NotFound();
        }

        return View(tour);
    }


    [AllowAnonymous]
    public async Task<IActionResult> Book()
    {
        var tours = await _context.Tours
            .Include(t => t.Destination)
            .Where(t => t.TourStatus == "Upcoming" && t.AvailableSeats > 0)
            .ToListAsync();

        return View(tours);
    }
}
