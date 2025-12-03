using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VirtualEventTicketing.Data;

namespace VirtualEventTicketing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var totalEvents = await _db.Events.CountAsync();
            var totalCategories = await _db.Categories.CountAsync();
            var lowStock = await _db.Events.CountAsync(e => e.AvailableTickets < 5);

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.LowStock = lowStock;
            return View();
        }

        public IActionResult Overview() => RedirectToAction("Overview", "Events");

        [Route("Home/Error500")]
        public IActionResult Error500()
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogError("Unhandled error occurred. TraceId={TraceId}", requestId);
            Log.Error("Unhandled error occurred. TraceId={TraceId}", requestId);
            return View("Error500");
        }

        public IActionResult StatusCode(int code)
        {
            if (code == 404)
            {
                _logger.LogWarning("404 Not Found for path {Path}", HttpContext.Request.Path);
                Log.Warning("404 Not Found for path {Path}", HttpContext.Request.Path);
                return View("NotFound");
            }

            ViewBag.StatusCode = code;
            return View("Error500");
        }
    }
}