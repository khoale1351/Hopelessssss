using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travel.Data;
using Travel.Repositories;

namespace Travel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _unitOfWork.Users.GetAllAsync();
            var totalTours = await _unitOfWork.Tours.GetAllAsync();
            var totalBookings = await _unitOfWork.Bookings.GetAllAsync();
            var totalReviews = await _unitOfWork.Reviews.GetAllAsync();
            ViewBag.TotalUsers = totalUsers.Count();
            ViewBag.TotalTours = totalTours.Count();
            ViewBag.TotalBookings = totalBookings.Count();
            ViewBag.TotalReviews = totalReviews.Count();
            return View();
        }

        public IActionResult ManageUsers()
        {
            var users = _unitOfWork.Users.GetAllAsync();
            return View(users);
        }

        public IActionResult ManageTours()
        {
            var tours = _unitOfWork.Tours.GetAllAsync();
            return View(tours);
        }


        public IActionResult ManageBookings()
        {
            var bookings = _unitOfWork.Bookings.GetAllAsync();
            return View(bookings);
        }

        public IActionResult ManageReviews()
        {
            var reviews = _unitOfWork.Reviews.GetAllAsync();
            return View(reviews);
        }
    }
}
