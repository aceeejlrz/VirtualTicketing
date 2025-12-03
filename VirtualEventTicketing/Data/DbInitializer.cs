using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            var roles = new[] { "Admin", "Organizer", "Attendee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed default admin user
            var adminEmail = "admin@virtualtickets.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                await userManager.CreateAsync(adminUser, "Admin#12345");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            if (await db.Categories.AnyAsync()) return; // already seeded

            var categories = new List<Category>
            {
                new() { Name = "Webinar", Description = "Live online talks and tutorials" },
                new() { Name = "Concert", Description = "Live music and performances" },
                new() { Name = "Workshop", Description = "Hands-on training sessions" },
                new() { Name = "Conference", Description = "Multi-session professional events" }
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            var now = DateTimeOffset.UtcNow;
            var events = new List<Event>
            {
                new() { Title = "Intro to .NET 8", CategoryId = categories[0].CategoryId, StartDateTime = now.AddDays(7), TicketPrice = 0, AvailableTickets = 200 },
                new() { Title = "Lo-Fi Beats Live", CategoryId = categories[1].CategoryId, StartDateTime = now.AddDays(10), TicketPrice = 19.99m, AvailableTickets = 50 },
                new() { Title = "Frontend Workshop: CSS Grid", CategoryId = categories[2].CategoryId, StartDateTime = now.AddDays(5), TicketPrice = 9.99m, AvailableTickets = 4 },
                new() { Title = "Cloud Conf 2025 (Keynotes)", CategoryId = categories[3].CategoryId, StartDateTime = now.AddDays(30), TicketPrice = 49.00m, AvailableTickets = 500 }
            };
            db.Events.AddRange(events);
            await db.SaveChangesAsync();
        }
    }
}