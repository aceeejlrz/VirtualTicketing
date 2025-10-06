using Microsoft.AspNetCore.Mvc.Rendering;

namespace VirtualEventTicketing.ViewModels
{
    public class EventFilterVm
    {
        public string? SearchTitle { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int? CategoryId { get; set; }
        public string? Availability { get; set; } // "available" | "soldout" | null
        public string? SortBy { get; set; } // title|date|price
        public bool SortDesc { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = [];
    }
}

