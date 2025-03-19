using Microsoft.AspNetCore.Mvc;

namespace QuanLyTour.Controllers
{
    public class VeTauController : Controller
    {
		private readonly string _connectionString;
		private readonly ILogger<VeTauController> _logger;

		public VeTauController(ILogger<VeTauController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		public IActionResult Index()
        {
            return View();
        }
    }
}
