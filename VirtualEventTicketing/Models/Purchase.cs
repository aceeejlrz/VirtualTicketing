using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VirtualEventTicketing.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;

        [Required, StringLength(120)]
        public string GuestName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        public decimal TotalCost { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}