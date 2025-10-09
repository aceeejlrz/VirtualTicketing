using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketing.ViewModels
{
    public class PurchaseItemVm
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        [Range(1, 1000)] public int Quantity { get; set; } = 1;
    }

    public class PurchaseVm
    {
        [Required] public string GuestName { get; set; } = string.Empty;
        [Required, EmailAddress] public string GuestEmail { get; set; } = string.Empty;
        public List<PurchaseItemVm>? Items { get; set; } // Made nullable to address CS8618
        public decimal Total => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0; // Safe null handling
        public decimal UnitPrice { get; set; } // Added to match view
        public int Quantity { get; set; } = 1; // Added to match view
    }
}