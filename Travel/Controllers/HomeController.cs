using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel.Data;

namespace Travel.Controllers
{
    public class HomeController : Controller
    {
        private readonly TourismDbContext _context;

        public HomeController(TourismDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            var tours = _context.Tours.Take(3).ToList();
            return View(tours);
        }
        
    }
}