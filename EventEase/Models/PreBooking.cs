using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class PreBooking
    {
        [Key]
        public int PreBookingId { get; set; }

        [Display(Name = "Pre-Booking ID")]
        public string FormattedPreBookingId => "PB" + PreBookingId.ToString("D3");

        [Display(Name = "Request Date")]
        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        [StringLength(10)]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
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
        public DateTime? PreferredStartDate { get; set; }

        [Display(Name = "Preferred End Date")]
        [DataType(DataType.Date)]
        public DateTime? PreferredEndDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Waiting";

        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public int? ConvertedToBookingId { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties - make nullable with ?
        [ForeignKey("PreferredVenueId")]
        public virtual Venue? PreferredVenue { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? CreatedByAdmin { get; set; }
    }
}