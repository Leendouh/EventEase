using EventEase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEase.Models.ViewModels
{
    public class VenueSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? LocationFilter { get; set; }
        public int? MinCapacity { get; set; }
        public bool? IsAvailable { get; set; }
        public List<Venue> Venues { get; set; } = new();
        public SelectList? LocationList { get; set; }
    }

    public class EventSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? VenueId { get; set; }
        public bool? ShowUnconfirmed { get; set; }
        public List<Event> Events { get; set; } = new();
        public SelectList? VenueList { get; set; }
    }

    public class BookingSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int? VenueId { get; set; }
        public int? EventId { get; set; }
        public List<Booking> Bookings { get; set; } = new();
        public SelectList? VenueList { get; set; }
        public SelectList? EventList { get; set; }
        public SelectList? StatusList => new SelectList(
            new[] { "Pending", "Confirmed", "Cancelled", "Completed" }
        );
    }
}
