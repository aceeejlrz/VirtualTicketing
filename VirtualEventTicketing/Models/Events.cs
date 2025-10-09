using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualEventTicketing.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [DataType(DataType.DateTime)]
        // Second Changed
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd, hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset StartDateTime { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        [Range(0, 1000000)]
        public decimal TicketPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableTickets { get; set; }

        // FK -> Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        public bool IsSoldOut => AvailableTickets <= 0;
    }
}