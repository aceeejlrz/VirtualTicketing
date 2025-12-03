using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Services
{
    public class TicketPdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TicketPdfService> _logger;

        public TicketPdfService(IWebHostEnvironment env, ILogger<TicketPdfService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public byte[] GenerateTicketPdf(Purchase purchase, PurchaseItem item)
        {
            var qrBytes = Array.Empty<byte>();
            if (!string.IsNullOrWhiteSpace(item.QrCodePath))
            {
                var physical = Path.Combine(_env.WebRootPath, item.QrCodePath.TrimStart('/', '\\'));
                if (File.Exists(physical))
                {
                    qrBytes = File.ReadAllBytes(physical);
                }
            }

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Virtual Event Ticket").FontSize(24).Bold();
                        col.Item().Text($"Event: {item.Event?.Title}");
                        col.Item().Text($"Attendee: {purchase.GuestName} ({purchase.GuestEmail})");
                        col.Item().Text($"Date: {item.Event?.StartDateTime.LocalDateTime}");
                        col.Item().Text($"Quantity: {item.Quantity}");
                        col.Item().Text($"Total: {item.LineTotal:C}");

                        if (qrBytes.Length > 0)
                        {
                            col.Item().PaddingTop(20).AlignCenter().Image(qrBytes);
                        }
                    });
                });
            });

            var bytes = doc.GeneratePdf();
            _logger.LogInformation("Generated PDF ticket for purchase {PurchaseId}, item {PurchaseItemId}", purchase.PurchaseId, item.PurchaseItemId);
            return bytes;
        }
    }
}
