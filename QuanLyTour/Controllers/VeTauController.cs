using Microsoft.AspNetCore.Mvc;

namespace QuanLyTour.Controllers
{
    public class VeTauController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
