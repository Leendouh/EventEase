using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Display(Name = "Venue ID")]
        public string FormattedVenueId => "V" + VenueId.ToString("D3");

        [Required(ErrorMessage = "Venue name is required")]
        [Display(Name = "Venue Name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Venue name must be between 3 and 100 characters")]
        public string VenueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
        public int Capacity { get; set; }

        [Display(Name = "Venue Image")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? CreatedByAdmin { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
        public virtual ICollection<Event>? Events { get; set; }
    }
}