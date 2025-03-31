using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Models;
using Travel.Repositories.ToursRepository;
using Travel.Repositories.DestinationsRepository;
using Travel.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Travel.Controllers
{
    [Authorize]
    public class TourController : Controller
    {
        private readonly ITourRepository _tourRepository;
        private readonly IDestinationRepository _destinationRepository;

        public TourController(ITourRepository tourRepository, IDestinationRepository destinationRepository)
        {
            _tourRepository = tourRepository;
            _destinationRepository = destinationRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var tours = await _tourRepository.GetAllAsync();
            var tourViewModels = tours.Select(t => new TourViewModel
            {
                TourId = t.TourId,
                TourName = t.TourName,
                DestinationName = t.Destination.Name,
                Price = t.Price,
                StartDate = t.StartDate.ToString("dd/MM/yyyy"),
                EndDate = t.EndDate.ToString("dd/MM/yyyy"),
                AvailableSeats = t.AvailableSeats,
                TourType = t.TourType,
                TourStatus = t.TourStatus
            }).ToList();
            return View(tourViewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null)
                return NotFound();

            var tourViewModel = new TourViewModel
            {
                TourId = tour.TourId,
                DestinationId = tour.DestinationId,
                TourName = tour.TourName,
                DestinationName = tour.Destination.Name,
                Description = tour.Description,
                Price = tour.Price,
                StartDate = tour.StartDate.ToString("dd/MM/yyyy"),
                EndDate = tour.EndDate.ToString("dd/MM/yyyy"),
                AvailableSeats = tour.AvailableSeats,
                TourType = tour.TourType,
                TourStatus = tour.TourStatus,
                Duration = tour.Duration
            };
            return View(tourViewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var destinations = await _destinationRepository.GetAllAsync();
            ViewBag.Destinations = new SelectList(destinations, "DestinationId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(Tour tour)
        {
            if (ModelState.IsValid)
            {
                tour.CreatedAt = DateTime.UtcNow;
                await _tourRepository.AddAsync(tour);
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null)
                return NotFound();
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, Tour tour)
        {
            if (id != tour.TourId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _tourRepository.UpdateAsync(tour);
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null)
                return NotFound();
            return View(tour);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour != null)
            {
                await _tourRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        // Thêm chức năng Book từ code mới
        public IActionResult Book()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(string tour, string name, string email, string phone)
        {
            ViewBag.Message = $"Thank you, {name}! Your booking for Tour ID {tour} has been received.";
            return View();
        }
    }
}
