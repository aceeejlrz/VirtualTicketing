using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using QuestPDF.Infrastructure;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.Services;
using Xunit;

namespace VirtualEventTicketing.Tests.Services
{
    public class TicketPdfServiceTests
    {
        [Fact]
        public void GenerateTicketPdf_ReturnsBytes()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.WebRootPath).Returns(Directory.GetCurrentDirectory());
            var logger = new LoggerFactory().CreateLogger<TicketPdfService>();

            var svc = new TicketPdfService(envMock.Object, logger);
            var purchase = new Purchase { PurchaseId = 1, GuestName = "Test", GuestEmail = "test@example.com" };
            var item = new PurchaseItem { PurchaseItemId = 1, Event = new Event { Title = "Test", StartDateTime = DateTimeOffset.UtcNow }, Quantity = 1, LineTotal = 10 };

            var bytes = svc.GenerateTicketPdf(purchase, item);

            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }
    }
}
