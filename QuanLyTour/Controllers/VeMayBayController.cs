using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using QuanLyTour.Models.VeMayBay;
using X.PagedList.Extensions;

namespace QuanLyTour.Controllers
{
    public class VeMayBayController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<VeMayBayController> _logger;
        public IActionResult Index()
        {
            return View();
        }
    }
}
