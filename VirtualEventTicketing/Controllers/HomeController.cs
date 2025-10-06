using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;

namespace VirtualEventTicketing.Controllers
{
    public class HomeController(ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var totalEvents = await db.Events.CountAsync();
            var totalCategories = await db.Categories.CountAsync();
            var lowStock = await db.Events.CountAsync(e => e.AvailableTickets < 5);

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.LowStock = lowStock;
            return View();
        }

        public IActionResult Overview() => RedirectToAction("Overview", "Events");
    }
}