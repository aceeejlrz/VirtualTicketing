// FOURTH CHANGED
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.ViewModels;

namespace VirtualEventTicketing.Controllers
{ 
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<EventsController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Events
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] EventFilterVm filter)
        {
            // Initialize base query with category join
            var query = _db.Events.Include(e => e.Category).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTitle))
            {
                var term = filter.SearchTitle.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term));
            }
            if (filter.From.HasValue)
            {
                var fromUtc = filter.From.Value.ToUniversalTime();
                query = query.Where(e => e.StartDateTime >= fromUtc);
            }
            if (filter.To.HasValue)
            {
                var toUtc = filter.To.Value.ToUniversalTime();
                query = query.Where(e => e.StartDateTime <= toUtc);
            }
            if (filter.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
            if (!string.IsNullOrEmpty(filter.Availability))
            {
                if (filter.Availability.Equals("available", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(e => e.AvailableTickets > 0);
                else if (filter.Availability.Equals("soldout", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(e => e.AvailableTickets <= 0);
            }

            // Apply sorting based on filter
            filter.SortBy = string.IsNullOrEmpty(filter.SortBy) ? "date" : filter.SortBy;
            var desc = filter.SortDesc;
            query = filter.SortBy.ToLower() switch
            {
                "title" => desc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
                "price" => desc ? query.OrderByDescending(e => e.TicketPrice) : query.OrderBy(e => e.TicketPrice),
                _ => desc ? query.OrderByDescending(e => e.StartDateTime) : query.OrderBy(e => e.StartDateTime)
            };

            // Load categories for dropdown
            filter.Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
            
            ViewBag.Filter = filter;
            var events = await query.AsNoTracking().ToListAsync();
            return View(events);
        }

        // GET: /Events/Overview (summary page)
        [AllowAnonymous]
        public async Task<IActionResult> Overview()
        {
            var totalEvents = await _db.Events.CountAsync();
            var totalCategories = await _db.Categories.CountAsync();
            var lowStockEvents = await _db.Events.Include(e => e.Category)
                .Where(e => e.AvailableTickets < 5)
                .OrderBy(e => e.AvailableTickets)
                .ToListAsync();
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalCategories = totalCategories;
            return View(lowStockEvents);
        }

        // GET: /Events/Create
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetCategoriesAsync();
            return View(new Event { StartDateTime = DateTimeOffset.UtcNow.AddDays(1) });
        }

        // POST: /Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Create(Event model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategoriesAsync();
                return View(model);
            }

            // Convert date to UTC
            model.StartDateTime = model.StartDateTime.ToUniversalTime();
            model.EndDateTime = model.EndDateTime?.ToUniversalTime();

            // Link organizer to current user
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                model.OrganizerId = userId;
            }

            _db.Events.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Edit/5
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (!CanManageEvent(ev)) return Forbid();

            ViewBag.Categories = await GetCategoriesAsync();
            return View(ev);
        }
        
        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Edit(int id, Event model)
        {
            if (id != model.EventId) return BadRequest();
    
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategoriesAsync();
                return View(model);
            }
    
            // Retrieve the existing event from database
            var existingEvent = await _db.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            if (!CanManageEvent(existingEvent)) return Forbid();
            
            existingEvent.Title = model.Title;
            existingEvent.CategoryId = model.CategoryId;
            existingEvent.StartDateTime = model.StartDateTime.ToUniversalTime();
            existingEvent.EndDateTime = model.EndDateTime?.ToUniversalTime();
            existingEvent.TicketPrice = model.TicketPrice;
            existingEvent.AvailableTickets = model.AvailableTickets;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        

        // GET: /Events/Delete/5
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            if (!CanManageEvent(ev)) return Forbid();
            return View(ev);
        }

        // POST: /Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev != null)
            {
                if (!CanManageEvent(ev)) return Forbid();

                _db.Events.Remove(ev);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _db.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // Retrieve categories for dropdown
        private async Task<IEnumerable<SelectListItem>> GetCategoriesAsync()
        {
            return await _db.Categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
        }

        private bool CanManageEvent(Event ev)
        {
            if (User.IsInRole("Admin")) return true;
            var userId = _userManager.GetUserId(User);
            return !string.IsNullOrEmpty(userId) && ev.OrganizerId == userId;
        }

        // AJAX: Live search partial
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string term)
        {
            var query = _db.Events.Include(e => e.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term));
            }

            var events = await query.OrderBy(e => e.StartDateTime).Take(20).ToListAsync();
            return PartialView("_EventPartial", events);
        }

        // Analytics dashboard
        [Authorize(Roles = "Admin,Organizer")]
        public IActionResult MyAnalytics()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpGet]
        public async Task<IActionResult> SalesByCategory()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _db.PurchaseItems
                .Include(pi => pi.Event)!
                .ThenInclude(e => e!.Category)
                .AsQueryable();

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(pi => pi.Event!.OrganizerId == userId);
            }

            var data = await query
                .GroupBy(pi => pi.Event!.Category!.Name)
                .Select(g => new { Category = g.Key, Tickets = g.Sum(pi => pi.Quantity) })
                .ToListAsync();

            return Json(data);
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpGet]
        public async Task<IActionResult> MonthlyRevenue()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _db.PurchaseItems
                .Include(pi => pi.Event)
                .AsQueryable();

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(pi => pi.Event!.OrganizerId == userId);
            }

            var data = await query
                .GroupBy(pi => new { pi.Purchase!.PurchaseDate.Year, pi.Purchase.PurchaseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(pi => pi.LineTotal)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Json(data);
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpGet]
        public async Task<IActionResult> TopEvents()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _db.PurchaseItems
                .Include(pi => pi.Event)
                .AsQueryable();

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(pi => pi.Event!.OrganizerId == userId);
            }

            var data = await query
                .GroupBy(pi => new { pi.EventId, pi.Event!.Title })
                .Select(g => new { EventId = g.Key.EventId, Title = g.Key.Title, Tickets = g.Sum(pi => pi.Quantity), Revenue = g.Sum(pi => pi.LineTotal) })
                .OrderByDescending(x => x.Tickets)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
    }
}