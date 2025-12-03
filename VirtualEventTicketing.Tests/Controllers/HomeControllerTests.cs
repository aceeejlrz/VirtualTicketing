using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualEventTicketing.Controllers;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;
using Xunit;

namespace VirtualEventTicketing.Tests.Controllers
{
    public class HomeControllerTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new ApplicationDbContext(options);
            db.Categories.Add(new Category { Name = "Test" });
            db.Events.Add(new Event { Title = "Test Event", CategoryId = 1, TicketPrice = 0, AvailableTickets = 1, StartDateTime = DateTimeOffset.UtcNow });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            using var db = CreateDbContext();
            var logger = new LoggerFactory().CreateLogger<HomeController>();
            var controller = new HomeController(db, logger);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view);
        }
    }
}
