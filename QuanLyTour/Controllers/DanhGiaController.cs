using Microsoft.AspNetCore.Mvc;

namespace QuanLyTour.Controllers
{
    public class DanhGiaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
