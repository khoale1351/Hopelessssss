using Microsoft.AspNetCore.Mvc;

namespace Travel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}