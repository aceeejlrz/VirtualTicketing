using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualEventTicketing.Models
{
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }

        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal LineTotal { get; set; }

        public int? Rating { get; set; }

        [StringLength(260)]
        public string? QrCodePath { get; set; }
    }
}