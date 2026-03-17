using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEase.Models.ViewModels
{
    public class PreBookingSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PreferredVenueId { get; set; }
        public List<PreBooking> PreBookings { get; set; } = new();
        public SelectList? StatusList { get; set; }
        public SelectList? VenueList { get; set; }
    }

    public class CreatePreBookingViewModel
    {
        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event name is required")]
        [Display(Name = "Event Name")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Display(Name = "Event Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string EventDescription { get; set; } = string.Empty;

        [Display(Name = "Expected Attendees")]
        [Range(1, 10000)]
        public int? ExpectedAttendees { get; set; }

        [Display(Name = "Preferred Venue")]
        public int? PreferredVenueId { get; set; }

        [Display(Name = "Preferred Start Date")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime? PreferredStartDate { get; set; }

        [Display(Name = "Preferred End Date")]
        [DataType(DataType.Date)]
        [DateGreaterThan("PreferredStartDate", ErrorMessage = "End date must be after start date")]
        public DateTime? PreferredEndDate { get; set; }

        [Display(Name = "Additional Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    public class PreBookingDetailsViewModel
    {
        public PreBooking PreBooking { get; set; } = new();
        public List<Booking>? ConvertedBookings { get; set; }
        public List<Venue>? AvailableVenues { get; set; }
    }

    public class ConvertPreBookingViewModel
    {
        public int PreBookingId { get; set; }
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
    public class EditPreBookingViewModel
    {
        public int PreBookingId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event name is required")]
        [Display(Name = "Event Name")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Display(Name = "Event Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string EventDescription { get; set; } = string.Empty;

        [Display(Name = "Expected Attendees")]
        [Range(1, 10000)]
        public int? ExpectedAttendees { get; set; }

        [Display(Name = "Preferred Venue")]
        public int? PreferredVenueId { get; set; }

        [Display(Name = "Preferred Start Date")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime? PreferredStartDate { get; set; }

        [Display(Name = "Preferred End Date")]
        [DataType(DataType.Date)]
        [DateGreaterThan("PreferredStartDate", ErrorMessage = "End date must be after start date")]
        public DateTime? PreferredEndDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Waiting";

        [Display(Name = "Additional Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}