using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.ViewModels;

namespace VirtualEventTicketing.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ILogger<DashboardController> logger)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var upcoming = await _db.PurchaseItems
                .Include(pi => pi.Event)
                .Include(pi => pi.Purchase)
                .Where(pi => pi.Purchase!.UserId == userId && pi.Event!.StartDateTime >= DateTimeOffset.UtcNow)
                .OrderBy(pi => pi.Event!.StartDateTime)
                .ToListAsync();

            var past = await _db.PurchaseItems
                .Include(pi => pi.Event)
                .Include(pi => pi.Purchase)
                .Where(pi => pi.Purchase!.UserId == userId && pi.Event!.StartDateTime < DateTimeOffset.UtcNow)
                .OrderByDescending(pi => pi.Event!.StartDateTime)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                UpcomingTickets = upcoming,
                PastTickets = past,
                MyEvents = new List<EventRevenueVm>(),
                Profile = await BuildProfileAsync(userId)
            };

            if (User.IsInRole("Organizer") || User.IsInRole("Admin"))
            {
                vm.MyEvents = await _db.PurchaseItems
                    .Include(pi => pi.Event)
                    .Include(pi => pi.Purchase)
                    .Where(pi => pi.Event!.OrganizerId == userId)
                    .GroupBy(pi => new { pi.EventId, pi.Event!.Title })
                    .Select(g => new EventRevenueVm
                    {
                        EventId = g.Key.EventId,
                        Title = g.Key.Title,
                        TicketsSold = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.LineTotal)
                    })
                    .OrderByDescending(e => e.Revenue)
                    .ToListAsync();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileVm vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;

            if (vm.ProfileImageFile != null && vm.ProfileImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"profile-{user.Id}-{Guid.NewGuid():N}{Path.GetExtension(vm.ProfileImageFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await vm.ProfileImageFile.CopyToAsync(stream);
                }
                user.ProfileImagePath = $"/uploads/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Profile updated for user {UserId}", user.Id);
            Log.Information("Profile updated for user {UserId}", user.Id);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateTicket(int purchaseItemId, int rating)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _db.PurchaseItems
                .Include(pi => pi.Purchase)
                .Include(pi => pi.Event)
                .FirstOrDefaultAsync(pi => pi.PurchaseItemId == purchaseItemId && pi.Purchase!.UserId == userId);
            if (item == null) return NotFound();

            rating = Math.Clamp(rating, 1, 5);
            item.Rating = rating;
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} rated event {EventId} with {Rating}", userId, item.EventId, rating);
            Log.Information("User {UserId} rated event {EventId} with {Rating}", userId, item.EventId, rating);

            return RedirectToAction(nameof(Index));
        }

        private async Task<ProfileVm> BuildProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
            return new ProfileVm
            {
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfileImagePath = user.ProfileImagePath
            };
        }
    }
}
