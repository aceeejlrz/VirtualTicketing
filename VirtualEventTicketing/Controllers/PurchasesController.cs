using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.ViewModels;

namespace VirtualEventTicketing.Controllers
{
    public class PurchasesController(ApplicationDbContext db) : Controller
    {
        // GET: /Purchases/Create?eventId=5 (single) or none (multi-select from list)
        public async Task<IActionResult> Create(int? eventId)
        {
            var vm = new PurchaseVm();
            if (eventId.HasValue)
            {
                var ev = await db.Events.FindAsync(eventId.Value);
                if (ev == null) return NotFound();
                vm.Items.Add(new PurchaseItemVm { EventId = ev.EventId, Title = ev.Title, UnitPrice = ev.TicketPrice, Quantity = 1 });
            }
            else
            {
                // Default: pre-load first 2 available events for demo convenience
                var evs = await db.Events.Where(e => e.AvailableTickets > 0).OrderBy(e => e.StartDateTime).Take(2).ToListAsync();
                foreach (var e in evs)
                    vm.Items.Add(new PurchaseItemVm { EventId = e.EventId, Title = e.Title, UnitPrice = e.TicketPrice, Quantity = 1 });
            }
            return View(vm);
        }

        // POST: /Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseVm vm)
        {
            if (!ModelState.IsValid || vm.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Select at least one event.");
                return View(vm);
            }

            // Load events and validate stock
            var eventIds = vm.Items.Select(i => i.EventId).ToList();
            var events = await db.Events.Where(e => eventIds.Contains(e.EventId)).ToListAsync();

            foreach (var item in vm.Items)
            {
                var ev = events.First(e => e.EventId == item.EventId);
                if (item.Quantity <= 0) { ModelState.AddModelError(string.Empty, $"Invalid quantity for {ev.Title}."); return View(vm); }
                if (ev.AvailableTickets < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Not enough tickets for {ev.Title}. Available: {ev.AvailableTickets}");
                    return View(vm);
                }
            }

            var purchase = new Purchase
            {
                GuestName = vm.GuestName,
                GuestEmail = vm.GuestEmail,
                PurchaseDate = DateTimeOffset.UtcNow,
                TotalCost = vm.Total
            };
            db.Purchases.Add(purchase);
            await db.SaveChangesAsync();

            // Create items & decrement stock
            foreach (var item in vm.Items)
            {
                var ev = events.First(e => e.EventId == item.EventId);
                var lineTotal = item.UnitPrice * item.Quantity;
                db.PurchaseItems.Add(new PurchaseItem
                {
                    PurchaseId = purchase.PurchaseId,
                    EventId = ev.EventId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = lineTotal
                });
                ev.AvailableTickets -= item.Quantity;
            }
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Confirm), new { id = purchase.PurchaseId });
        }

        // GET: /Purchases/Confirm/5
        public async Task<IActionResult> Confirm(int id)
        {
            var p = await db.Purchases
                .Include(x => x.Items)
                .ThenInclude(i => i.Event)
                .FirstOrDefaultAsync(x => x.PurchaseId == id);
            if (p == null) return NotFound();
            return View(p);
        }
    }
}
