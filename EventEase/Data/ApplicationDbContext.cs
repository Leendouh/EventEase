using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;  
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties represent database tables
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PreBooking> PreBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // VENUE CONFIGURATION
            // =============================================
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.ToTable("Venues");
                entity.HasKey(v => v.VenueId);

                entity.Property(v => v.VenueName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(v => v.Location)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(v => v.Capacity)
                    .IsRequired();

                entity.Property(v => v.ImageUrl)
                    .HasMaxLength(500);

                entity.Property(v => v.Description)
                    .HasMaxLength(500);

                entity.Property(v => v.IsAvailable)
                    .HasDefaultValue(true);

                entity.Property(v => v.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");
            });

            // =============================================
            // EVENT CONFIGURATION
            // =============================================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events");
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.EventDate)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ExpectedAttendees);

                entity.Property(e => e.IsVenueConfirmed)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Venue)
                    .WithMany(v => v.Events)
                    .HasForeignKey(e => e.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByAdmin)
                    .WithMany(a => a.Events)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =============================================
            // BOOKING CONFIGURATION
            // =============================================
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(b => b.BookingId);

                entity.Property(b => b.BookingNumber)
                    .HasComputedColumnSql("'BKG-' + RIGHT('00000' + CAST(BookingId AS VARCHAR(5)), 5)");

                entity.Property(b => b.BookingDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.StartDate)
                    .IsRequired();

                entity.Property(b => b.EndDate)
                    .IsRequired();

                entity.Property(b => b.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(b => b.Notes)
                    .HasMaxLength(500);

                entity.Property(b => b.ExpectedAttendees);

                entity.Property(b => b.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(b => b.CustomerSurname)
                    .HasMaxLength(100);

                entity.Property(b => b.CustomerIDNumber)
                    .HasMaxLength(20);

                entity.Property(b => b.CustomerPhone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(b => b.CustomerEmail)
                    .HasMaxLength(100);

                entity.Property(b => b.CustomerAddress)
                    .HasMaxLength(200);

                entity.Property(b => b.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(b => new { b.VenueId, b.StartDate, b.EndDate })
                    .IsUnique()
                    .HasDatabaseName("IX_Unique_Venue_Booking");

                entity.HasOne(b => b.Event)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(b => b.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Venue)
                    .WithMany(v => v.Bookings)
                    .HasForeignKey(b => b.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.PreBooking)
                    .WithMany()
                    .HasForeignKey(b => b.PreBookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.CreatedByAdmin)
                    .WithMany(a => a.Bookings)
                    .HasForeignKey(b => b.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =============================================
            // APPLICATION USER CONFIGURATION (formerly ADMIN)
            // =============================================
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Admins"); // Use your existing table name
                entity.HasKey(a => a.Id);

                // Map the properties from Admin to ApplicationUser
                entity.Property(a => a.AdminId).HasColumnName("AdminId");
                entity.Property(a => a.UserName).HasColumnName("Username").HasMaxLength(50); // Fixed: UserName not Username
                entity.Property(a => a.PasswordHash).HasColumnName("PasswordHash");
                entity.Property(a => a.FullName).HasColumnName("FullName").HasMaxLength(100);
                entity.Property(a => a.Email).HasColumnName("Email").HasMaxLength(100);
                entity.Property(a => a.Role).HasColumnName("Role").HasMaxLength(20);
                entity.Property(a => a.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(a => a.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("GETDATE()");
                entity.Property(a => a.LastLoginDate).HasColumnName("LastLoginDate");
                entity.Property(a => a.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(10);
                entity.Property(a => a.IsFirstLogin).HasColumnName("IsFirstLogin").HasDefaultValue(true);
                entity.Property(a => a.LastPasswordChangeDate).HasColumnName("LastPasswordChangeDate");
                entity.Property(a => a.CreatedBy).HasColumnName("CreatedBy");

                // Identity required properties
                entity.Property(a => a.NormalizedUserName).HasMaxLength(256);
                entity.Property(a => a.NormalizedEmail).HasMaxLength(256);
                entity.Property(a => a.SecurityStamp).HasMaxLength(256);
                entity.Property(a => a.ConcurrencyStamp).HasMaxLength(256);

                // Relationships - Fixed navigation property names
                entity.HasOne(a => a.CreatedByAdmin)
                    .WithMany()
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Bookings relationship - Booking uses CreatedByAdmin not CreatedBy
                entity.HasMany(a => a.Bookings)
                    .WithOne(b => b.CreatedByAdmin)  // Changed from CreatedBy to CreatedByAdmin
                    .HasForeignKey(b => b.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Venues relationship - Venue uses CreatedByAdmin not CreatedBy
                entity.HasMany(a => a.Venues)
                    .WithOne(v => v.CreatedByAdmin)  // Changed from CreatedBy to CreatedByAdmin
                    .HasForeignKey(v => v.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Events relationship - Event uses CreatedByAdmin
                entity.HasMany(a => a.Events)
                    .WithOne(e => e.CreatedByAdmin)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // =============================================
            // PRE-BOOKING CONFIGURATION
            // =============================================
            modelBuilder.Entity<PreBooking>(entity =>
            {
                entity.ToTable("PreBookings");
                entity.HasKey(p => p.PreBookingId);

                entity.Property(p => p.RequestDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.CustomerPhone)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(p => p.CustomerEmail)
                    .HasMaxLength(100);

                entity.Property(p => p.EventName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.EventDescription)
                    .HasMaxLength(500);

                entity.Property(p => p.ExpectedAttendees);

                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Waiting");

                entity.Property(p => p.Notes)
                    .HasMaxLength(500);

                entity.Property(p => p.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(p => p.PreferredVenue)
                    .WithMany()
                    .HasForeignKey(p => p.PreferredVenueId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CreatedByAdmin)
                    .WithMany()
                    .HasForeignKey(p => p.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =============================================
            // SEED DATA
            // =============================================

            // Seed sample venues
            modelBuilder.Entity<Venue>().HasData(
                new Venue
                {
                    VenueId = 1,
                    VenueName = "Grand Ballroom",
                    Location = "Sandton, Johannesburg",
                    Capacity = 500,
                    ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=500",
                    Description = "Elegant ballroom with crystal chandeliers and sprung dance floor",
                    IsAvailable = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Venue
                {
                    VenueId = 2,
                    VenueName = "Garden Pavilion",
                    Location = "Cape Town",
                    Capacity = 150,
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=500",
                    Description = "Outdoor pavilion surrounded by beautiful gardens",
                    IsAvailable = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Venue
                {
                    VenueId = 3,
                    VenueName = "Conference Hall A",
                    Location = "Midrand",
                    Capacity = 200,
                    ImageUrl = "https://images.unsplash.com/photo-1431540015161-0bf86838fb31?w=500",
                    Description = "Modern conference facility with AV equipment",
                    IsAvailable = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Venue
                {
                    VenueId = 4,
                    VenueName = "Rooftop Terrace",
                    Location = "Durban",
                    Capacity = 100,
                    ImageUrl = "https://images.unsplash.com/photo-1517457378542-03527b9f5b6d?w=500",
                    Description = "Rooftop venue with ocean views",
                    IsAvailable = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Venue
                {
                    VenueId = 5,
                    VenueName = "Intimate Gallery",
                    Location = "Pretoria",
                    Capacity = 50,
                    ImageUrl = "https://images.unsplash.com/photo-1520031442192-dcef72198f63?w=500",
                    Description = "Art gallery space for small gatherings",
                    IsAvailable = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                }
            );

            // Seed sample events
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = 1,
                    EventName = "Annual Tech Summit 2024",
                    EventDate = new DateTime(2024, 11, 15),
                    Description = "Technology conference featuring local innovators",
                    ExpectedAttendees = 450,
                    VenueId = 1,
                    IsVenueConfirmed = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Event
                {
                    EventId = 2,
                    EventName = "Summer Wedding Fair",
                    EventDate = new DateTime(2024, 10, 20),
                    Description = "Bridal showcase and wedding vendor exhibition",
                    ExpectedAttendees = 120,
                    VenueId = 2,
                    IsVenueConfirmed = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Event
                {
                    EventId = 3,
                    EventName = "New Year's Eve Gala",
                    EventDate = new DateTime(2024, 12, 31),
                    Description = "Countdown celebration with live entertainment",
                    ExpectedAttendees = 90,
                    VenueId = null,
                    IsVenueConfirmed = false,
                    CreatedDate = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}