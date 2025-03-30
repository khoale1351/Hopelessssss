using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Travel.Models;
using Travel.Repositories;

namespace Travel.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Liệt kê tất cả review của user hiện tại
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var reviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId);
            return View(reviews);
        }

        // Xem chi tiết review
        public async Task<IActionResult> Details(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }

        public async Task<IActionResult> Create(int tourId)
        {
            var tour = await _unitOfWork.Tours.GetByIdAsync(tourId);
            if (tour == null)
            {
                return NotFound();
            }
            ViewBag.Tour = tour;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tourId, Review review)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tour = await _unitOfWork.Tours.GetByIdAsync(tourId);
            if (tour == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                review.TourId = tourId;
                review.UserId = userId;
                review.ReviewDate = DateTime.UtcNow;
                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tour = tour;
            return View(review);
        }

        
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review updatedReview)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (id != updatedReview.ReviewId)
            {
                return BadRequest();
            }

            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                review.Rating = updatedReview.Rating;
                review.Comment = updatedReview.Comment;
                review.ReviewDate = DateTime.UtcNow;
                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); 
            }

            return View(updatedReview);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            await _unitOfWork.Reviews.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
