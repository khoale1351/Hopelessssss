using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.Repositories;
using Travel.ViewModels;
using System.Text;

public class TourController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TourismDbContext _context;

    public TourController(IUnitOfWork unitOfWork, TourismDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public IActionResult Create()
    {
        var model = new TourViewModel
        {
            DestinationOptions = _context.Destinations
                .Select(d => new SelectListItem
                {
                    Value = d.DestinationId.ToString(),
                    Text = d.Name
                })
                .ToList()
        };

        return View(model);
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

    // POST: Tour/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create(TourViewModel model)
    {
        if (ModelState.IsValid)
        {
            var tour = new Tour
            {
                DestinationId = model.DestinationId,
                TourName = model.TourName,
                Description = model.Description,
                Price = model.Price,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                AvailableSeats = model.AvailableSeats,
                TourType = model.TourType,
                TourStatus = model.TourStatus,
                Duration = model.Duration
            };

            _context.Tours.Add(tour);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Re-populate the DestinationOptions in case of validation errors
        model.DestinationOptions = _context.Destinations
            .Select(d => new SelectListItem
            {
                Value = d.DestinationId.ToString(),
                Text = d.Name
            })
            .ToList();

        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour == null)
            return NotFound();

        var destinations = _context.Destinations
            .Select(d => new { d.DestinationId, d.Name })
            .ToList();

        ViewBag.Destinations = destinations;
        return View(tour);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, Tour tour, IFormFile ImageFile)
    {
        if (id != tour.TourId)
            return BadRequest();

        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", ImageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                tour.ImageUrl = "/images/" + ImageFile.FileName;
            }

            await _unitOfWork.Tours.UpdateAsync(tour);
            return RedirectToAction(nameof(Index));
        }

        return View(tour);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour == null)
            return NotFound();

        return View(tour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tour = await _unitOfWork.Tours.GetByIdAsync(id);
        if (tour != null)
        {
            await _unitOfWork.Tours.DeleteAsync(id);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult GetDestinations()
    {
        var destinations = _context.Destinations
            .Select(d => new { id = d.DestinationId, text = d.Name })
            .ToList();

        return Json(destinations);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult SearchTours(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return Json(new List<object>());
        }

        var keywordLower = keyword.ToLower();
        var tours = _context.Tours
            .Where(t => EF.Functions.Like(t.TourName.ToLower(), $"%{keywordLower}%"))
            .Select(t => new { t.TourId, t.TourName })
            .Take(5)
            .ToList();

        // Thêm logging để kiểm tra
        Console.WriteLine($"Keyword: {keywordLower}");
        Console.WriteLine($"Found {tours.Count} tours:");
        foreach (var tour in tours)
        {
            Console.WriteLine($"Tour: {tour.TourName}");
        }

        return Json(tours);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult FindTour(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return RedirectToAction("Index");
        }

        var keywordLower = keyword.ToLower();
        var tour = _context.Tours
            .FirstOrDefault(t => t.TourName.ToLower() == keywordLower);

        if (tour == null)
        {
            TempData["Error"] = "Tour not found.";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Details", new { id = tour.TourId });
    }
}