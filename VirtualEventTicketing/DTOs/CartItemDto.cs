namespace VirtualEventTicketing.DTOs
{
    public class CartItemDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
