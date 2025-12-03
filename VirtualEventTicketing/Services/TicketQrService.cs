using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QRCoder;
using System.Text;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Services
{
    public class TicketQrService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TicketQrService> _logger;

        public TicketQrService(IWebHostEnvironment env, ILogger<TicketQrService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> GenerateQrAsync(PurchaseItem item)
        {
            var payload = $"TICKET|P:{item.PurchaseId}|I:{item.PurchaseItemId}|E:{item.EventId}";

            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            var bytes = pngQr.GetGraphic(20);

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "tickets");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"ticket-{item.PurchaseId}-{item.PurchaseItemId}-{Guid.NewGuid():N}.png";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await File.WriteAllBytesAsync(filePath, bytes);

            var relativePath = $"/uploads/tickets/{fileName}";
            _logger.LogInformation("Generated QR for purchase item {PurchaseItemId} at {Path}", item.PurchaseItemId, relativePath);
            return relativePath;
        }
    }
}
