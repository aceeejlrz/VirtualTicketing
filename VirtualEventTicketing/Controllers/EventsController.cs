// FOURTH CHANGED
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.ViewModels;

namespace VirtualEventTicketing.Controllers
{
    public class EventsController(ApplicationDbContext db) : Controller
    {
        // GET: /Events
        public async Task<IActionResult> Index([FromQuery] EventFilterVm filter)
        {
            // Initialize base query with category join
            var query = db.Events.Include(e => e.Category).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTitle))
            {
                var term = filter.SearchTitle.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term));
            }
            if (filter.From.HasValue)
                query = query.Where(e => e.StartDateTime >= filter.From.Value);
            if (filter.To.HasValue)
                query = query.Where(e => e.StartDateTime <= filter.To.Value);
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
            filter.Categories = await db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
            
            ViewBag.Filter = filter;
            var events = await query.AsNoTracking().ToListAsync();
            return View(events);
        }

        // GET: /Events/Overview (summary page)
        public async Task<IActionResult> Overview()
        {
            var totalEvents = await db.Events.CountAsync();
            var totalCategories = await db.Categories.CountAsync();
            var lowStockEvents = await db.Events.Include(e => e.Category)
                .Where(e => e.AvailableTickets < 5)
                .OrderBy(e => e.AvailableTickets)
                .ToListAsync();
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalCategories = totalCategories;
            return View(lowStockEvents);
        }

        // GET: /Events/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetCategoriesAsync();
            return View(new Event { StartDateTime = DateTimeOffset.UtcNow.AddDays(1) });
        }

        // POST: /Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategoriesAsync();
                return View(model);
            }

            // Convert date to UTC
            model.StartDateTime = model.StartDateTime.ToUniversalTime();
            db.Events.Add(model);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            ViewBag.Categories = await GetCategoriesAsync();
            return View(ev);
        }

        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event model)
        {
            if (id != model.EventId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategoriesAsync();
                return View(model);
            }
            
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await db.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // POST: /Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await db.Events.FindAsync(id);
            if (ev != null)
            {
                db.Events.Remove(ev);
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ev = await db.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // Retrieve categories for dropdown
        private async Task<IEnumerable<SelectListItem>> GetCategoriesAsync()
        {
            return await db.Categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
