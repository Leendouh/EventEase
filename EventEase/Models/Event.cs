using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Display(Name = "Event ID")]
        public string FormattedEventId => "E" + EventId.ToString("D3");

        [Required(ErrorMessage = "Event name is required")]
        [Display(Name = "Event Name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Event name must be between 3 and 100 characters")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [FutureDate(ErrorMessage = "Event date must be in the future")]
        public DateTime EventDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Expected Attendees")]
        [Range(1, 10000, ErrorMessage = "Expected attendees must be between 1 and 10,000")]
        public int? ExpectedAttendees { get; set; }

        [Display(Name = "Venue")]
        public int? VenueId { get; set; }

        [Display(Name = "Venue Confirmed")]
        public bool IsVenueConfirmed { get; set; } = false;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? CreatedByAdmin { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                return date.Date >= DateTime.Now.Date;
            }
            return false;
        }
    }
}