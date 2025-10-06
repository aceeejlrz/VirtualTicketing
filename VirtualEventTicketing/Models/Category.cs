namespace VirtualEventTicketing.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}