using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.DTOs;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.Services;
using VirtualEventTicketing.ViewModels;

namespace VirtualEventTicketing.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly TicketQrService _qrService;
        private readonly TicketPdfService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PurchasesController> _logger;

        private const string CartSessionKey = "Cart";

        public PurchasesController(
            ApplicationDbContext db,
            TicketQrService qrService,
            TicketPdfService pdfService,
            UserManager<ApplicationUser> userManager,
            ILogger<PurchasesController> logger)
        {
            _db = db;
            _qrService = qrService;
            _pdfService = pdfService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Purchases/Create?eventId=5 (single) or none (multi-select from list or cart)
        public async Task<IActionResult> Create(int? eventId)
        {
            var vm = new PurchaseVm { Items = new List<PurchaseItemVm>() };

            if (eventId.HasValue)
            {
                var ev = await _db.Events.FindAsync(eventId.Value);
                if (ev == null) return NotFound();
                vm.Items.Add(new PurchaseItemVm { EventId = ev.EventId, Title = ev.Title, UnitPrice = ev.TicketPrice, Quantity = 1 });
            }
            else
            {
                var cart = GetCart();
                if (cart.Any())
                {
                    var ids = cart.Select(c => c.EventId).ToList();
                    var events = await _db.Events.Where(e => ids.Contains(e.EventId)).ToListAsync();
                    foreach (var c in cart)
                    {
                        var ev = events.First(e => e.EventId == c.EventId);
                        vm.Items.Add(new PurchaseItemVm
                        {
                            EventId = ev.EventId,
                            Title = ev.Title,
                            UnitPrice = ev.TicketPrice,
                            Quantity = c.Quantity
                        });
                    }
                }
                else
                {
                    // Default: pre-load first 2 available events for demo convenience
                    var evs = await _db.Events.Where(e => e.AvailableTickets > 0).OrderBy(e => e.StartDateTime).Take(2).ToListAsync();
                    foreach (var e in evs)
                        vm.Items.Add(new PurchaseItemVm { EventId = e.EventId, Title = e.Title, UnitPrice = e.TicketPrice, Quantity = 1 });
                }
            }
            return View(vm);
        }

        // POST: /Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseVm vm)
        {
            if (vm.Items == null || !vm.Items.Any() || !ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Select at least one event.");
                return View(vm);
            }

            // Load events and validate stock
            var eventIds = vm.Items.Select(i => i.EventId).ToList();
            var events = await _db.Events.Where(e => eventIds.Contains(e.EventId)).ToListAsync();

            foreach (var item in vm.Items)
            {
                var ev = events.First(e => e.EventId == item.EventId);
                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError(string.Empty, $"Invalid quantity for {ev.Title}.");
                    return View(vm);
                }
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
                TotalCost = vm.Total,
                UserId = _userManager.GetUserId(User)
            };
            _db.Purchases.Add(purchase);
            await _db.SaveChangesAsync();

            // Create items & decrement stock
            var createdItems = new List<PurchaseItem>();
            foreach (var item in vm.Items)
            {
                var ev = events.First(e => e.EventId == item.EventId);
                var lineTotal = item.UnitPrice * item.Quantity;
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.PurchaseId,
                    EventId = ev.EventId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = lineTotal
                };
                _db.PurchaseItems.Add(purchaseItem);
                createdItems.Add(purchaseItem);

                ev.AvailableTickets -= item.Quantity;
            }
            await _db.SaveChangesAsync();

            // Generate QR codes for each ticket item
            foreach (var item in createdItems)
            {
                item.QrCodePath = await _qrService.GenerateQrAsync(item);
            }
            await _db.SaveChangesAsync();

            // Clear cart after successful purchase
            HttpContext.Session.Remove(CartSessionKey);

            _logger.LogInformation("Purchase {PurchaseId} created for {Email} with total {Total}", purchase.PurchaseId, purchase.GuestEmail, purchase.TotalCost);
            Log.Information("Purchase {PurchaseId} created for {Email} with total {Total}", purchase.PurchaseId, purchase.GuestEmail, purchase.TotalCost);

            return RedirectToAction(nameof(Confirm), new { id = purchase.PurchaseId });
        }

        // GET: /Purchases/Confirm/5
        public async Task<IActionResult> Confirm(int id)
        {
            var p = await _db.Purchases
                .Include(x => x.Items)
                .ThenInclude(i => i.Event)
                .FirstOrDefaultAsync(x => x.PurchaseId == id);
            if (p == null) return NotFound();
            return View(p);
        }

        [Authorize]
        public async Task<IActionResult> TicketPdf(int purchaseId, int itemId)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var purchase = await _db.Purchases
                .Include(p => p.Items)
                .ThenInclude(i => i.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);
            if (purchase == null) return NotFound();

            if (!isAdmin && purchase.UserId != userId)
                return Forbid();

            var item = purchase.Items.FirstOrDefault(i => i.PurchaseItemId == itemId);
            if (item == null) return NotFound();

            var pdfBytes = _pdfService.GenerateTicketPdf(purchase, item);
            return File(pdfBytes, "application/pdf", $"ticket-{purchaseId}-{itemId}.pdf");
        }

        // CART API (session-based)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int eventId, int quantity = 1)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.EventId == eventId);
            if (existing == null)
            {
                cart.Add(new CartItemDto
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    UnitPrice = ev.TicketPrice,
                    Quantity = quantity
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            SaveCart(cart);

            var totalQty = cart.Sum(c => c.Quantity);
            var lowStockLeft = ev.AvailableTickets - (existing?.Quantity ?? quantity);

            return Json(new
            {
                success = true,
                itemCount = totalQty,
                lowStock = lowStockLeft > 0 && lowStockLeft < 5,
                remaining = lowStockLeft
            });
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int eventId, int quantity)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.EventId == eventId);
            if (existing == null) return NotFound();

            if (quantity <= 0)
                cart.Remove(existing);
            else
                existing.Quantity = quantity;

            SaveCart(cart);
            return Json(new { success = true, itemCount = cart.Sum(c => c.Quantity), total = cart.Sum(c => c.UnitPrice * c.Quantity) });
        }

        [HttpGet]
        public IActionResult CartSummary()
        {
            var cart = GetCart();
            return Json(new { itemCount = cart.Sum(c => c.Quantity), total = cart.Sum(c => c.UnitPrice * c.Quantity) });
        }

        private List<CartItemDto> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItemDto>();
            return JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
        }

        private void SaveCart(List<CartItemDto> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }
    }
}