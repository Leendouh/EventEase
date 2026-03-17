using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Display(Name = "Booking ID")]
        public string FormattedBookingId => "B" + BookingId.ToString("D3");

        [Display(Name = "Booking Number")]
        public string BookingNumber { get; set; } = string.Empty;

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [DateGreaterThan("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Expected Attendees")]
        [Range(1, 10000)]
        public int? ExpectedAttendees { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Customer Surname")]
        [StringLength(100)]
        public string CustomerSurname { get; set; } = string.Empty;

        [Display(Name = "ID/Passport Number")]
        [StringLength(20)]
        public string CustomerIDNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Display(Name = "Address")]
        [StringLength(200)]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; }

        public int? PreBookingId { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("PreBookingId")]
        public virtual PreBooking? PreBooking { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? CreatedByAdmin { get; set; }
    }

    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = (DateTime?)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? "End date must be after start date");
            }

            return ValidationResult.Success;
        }
    }
}