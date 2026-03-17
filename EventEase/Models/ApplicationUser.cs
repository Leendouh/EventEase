using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
            Bookings = new HashSet<Booking>();
            Venues = new HashSet<Venue>();
            Events = new HashSet<Event>();
        }

        // Keep the AdminId for backward compatibility
        public int AdminId { get; set; }

        // Custom properties from your Admin class
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public bool IsFirstLogin { get; set; } = true;
        public DateTime? LastPasswordChangeDate { get; set; }
        public string? CreatedBy { get; set; }

        // Computed property for formatted Admin ID
        [NotMapped]
        public string FormattedAdminId => "A" + AdminId.ToString("D3");

        // Navigation property for creator
        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? CreatedByAdmin { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Venue> Venues { get; set; }
        public virtual ICollection<Event> Events { get; set; }

        // Override IdentityUser properties with correct types
        public override DateTimeOffset? LockoutEnd { get; set; }
    }
}