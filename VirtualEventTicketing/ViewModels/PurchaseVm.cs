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
        public List<PurchaseItemVm> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}