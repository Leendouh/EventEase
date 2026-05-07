using System.ComponentModel.DataAnnotations;

namespace EventEase.Models.ViewModels
{
    public class BookingDetailsViewModel
    {
        public int BookingId { get; set; }
        
        [Required]
        [Display(Name = "Booking #")]
        public string BookingNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Display(Name = "Customer Surname")]
        public string? CustomerSurname { get; set; }
        
        [Display(Name = "Customer Phone")]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }
        
        [Display(Name = "ID Number")]
        public string? CustomerIDNumber { get; set; }
        
        [Display(Name = "Address")]
        public string? CustomerAddress { get; set; }
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
        
        [Display(Name = "Expected Attendees")]
        public int? ExpectedAttendees { get; set; }
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        
        // Event Information
        public int? EventId { get; set; }
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }
        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }
        [Display(Name = "Event Description")]
        public string? EventDescription { get; set; }
        [Display(Name = "Expected Event Attendees")]
        public int? EventExpectedAttendees { get; set; }
        
        // Venue Information
        public int? VenueId { get; set; }
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }
        [Display(Name = "Venue Location")]
        public string? VenueLocation { get; set; }
        [Display(Name = "Venue Capacity")]
        public int? VenueCapacity { get; set; }
        [Display(Name = "Venue Image")]
        public string? VenueImageUrl { get; set; }
        [Display(Name = "Venue Description")]
        public string? VenueDescription { get; set; }
        
        // Admin Information
        public string? CreatedByAdminId { get; set; }
        [Display(Name = "Created By")]
        public string? CreatedByAdminName { get; set; }
        
        // Additional computed properties
        [Display(Name = "Duration")]
        public string Duration => EndDate.Subtract(StartDate).TotalDays > 1 
            ? $"{EndDate.Subtract(StartDate).TotalDays:F0} days"
            : $"{EndDate.Subtract(StartDate).TotalHours:F0} hours";
        
        [Display(Name = "Venue Utilization")]
        public string VenueUtilization => ExpectedAttendees.HasValue && VenueCapacity.HasValue
            ? $"{(double)ExpectedAttendees.Value / VenueCapacity.Value * 100:F0}%"
            : "N/A";
        
        [Display(Name = "Status Badge")]
        public string StatusBadgeClass => Status.ToLower() switch
        {
            "confirmed" => "bg-success",
            "pending" => "bg-warning",
            "cancelled" => "bg-danger",
            "completed" => "bg-info",
            _ => "bg-secondary"
        };
    }
}
