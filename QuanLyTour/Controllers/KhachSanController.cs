using Microsoft.AspNetCore.Mvc;

namespace QuanLyTour.Controllers
{
    public class KhachSanController : Controller
    {
		private readonly string _connectionString;
		private readonly ILogger<KhachSanController> _logger;

		public KhachSanController(ILogger<KhachSanController> logger, IConfiguration configuration)
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
