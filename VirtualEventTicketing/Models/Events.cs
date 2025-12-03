using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualEventTicketing.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }
        
        [DataType(DataType.DateTime)]
        // Second Changed
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd, hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset StartDateTime { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd, hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? EndDateTime { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        [Range(0, 1000000)]
        public decimal TicketPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableTickets { get; set; }

        // FK -> Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? OrganizerId { get; set; }
        public ApplicationUser? Organizer { get; set; }

        [StringLength(260)]
        public string? ImagePath { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        public bool IsSoldOut => AvailableTickets <= 0;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}