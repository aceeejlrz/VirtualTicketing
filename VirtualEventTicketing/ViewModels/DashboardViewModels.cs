using Microsoft.AspNetCore.Http;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.ViewModels
{
    public class DashboardViewModel
    {
        public List<PurchaseItem> UpcomingTickets { get; set; } = new();
        public List<PurchaseItem> PastTickets { get; set; } = new();
        public List<EventRevenueVm> MyEvents { get; set; } = new();
        public ProfileVm Profile { get; set; } = new();
    }

    public class EventRevenueVm
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProfileVm
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImagePath { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
    }
}
