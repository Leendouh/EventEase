using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventEase.Controllers
{
    public class VenueController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<VenueController> _logger;

        public VenueController(ApplicationDbContext context, IAzureStorageService storageService, ILogger<VenueController> logger)
        {
            _context = context;
            _storageService = storageService;
            _logger = logger;
        }

        // GET: Venue with Search
        public async Task<IActionResult> Index(string searchTerm, string location, int? minCapacity, bool? isAvailable)
        {
            var query = _context.Venues.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v => v.VenueName.Contains(searchTerm) ||
                                         v.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(v => v.Location.Contains(location));
            }

            if (minCapacity.HasValue)
            {
                query = query.Where(v => v.Capacity >= minCapacity);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(v => v.IsAvailable == isAvailable);
            }

            var venues = await query.ToListAsync();

            var viewModel = new VenueSearchViewModel
            {
                SearchTerm = searchTerm,
                LocationFilter = location,
                MinCapacity = minCapacity,
                IsAvailable = isAvailable,
                Venues = venues,
                LocationList = new SelectList(await _context.Venues.Select(v => v.Location).Distinct().ToListAsync())
            };

            return View(viewModel);
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.Bookings!)
                    .ThenInclude(b => b.Event)
                .Include(v => v.Events)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,Description,IsAvailable")] Venue venue, IFormFile? imageFile)
        {
            _logger.LogInformation("=== VENUE CREATE START ===");
            _logger.LogInformation("Venue data: {VenueName}, {Location}, {Capacity}", venue.VenueName, venue.Location, venue.Capacity);
            _logger.LogInformation("Image file: {FileName}, Length: {Length}", imageFile?.FileName, imageFile?.Length);
            _logger.LogInformation("ModelState valid: {IsValid}", ModelState.IsValid);
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState errors: {Errors}", string.Join(", ", errors));
            }

            // Handle image upload properly
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    _logger.LogInformation("Processing image upload: {FileName}, Size: {Length}", imageFile.FileName, imageFile.Length);
                    
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("imageFile", "Invalid file type. Please upload JPG, PNG, GIF, or WebP images.");
                    }
                    
                    // Validate file size (max 5MB)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("imageFile", "File size too large. Maximum size is 5MB.");
                    }
                    
                    if (ModelState.IsValid)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "venues");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            venue.ImageUrl = imageUrl;
                            _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
                        }
                        else
                        {
                            venue.ImageUrl = "https://via.placeholder.com/300x200?text=Venue+Image";
                            _logger.LogWarning("Image upload failed, using fallback image");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image: {Message}", ex.Message);
                    venue.ImageUrl = "https://via.placeholder.com/300x200?text=Venue+Image";
                }
            }
            else
            {
                // Use default image if no image uploaded
                venue.ImageUrl = "https://via.placeholder.com/300x200?text=Venue+Image";
                _logger.LogInformation("No image uploaded, using default placeholder");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    venue.CreatedDate = DateTime.Now;
                    venue.CreatedBy = GetCurrentUserId();

                    _context.Add(venue);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Venue created successfully: {venue.VenueName} (ID: {venue.VenueId})");
                    _logger.LogInformation($"Saved ImageUrl: {venue.ImageUrl}");
                    TempData["Success"] = $"Venue '{venue.VenueName}' created successfully! Image: {(venue.ImageUrl.Contains("placeholder") ? "Placeholder" : "Uploaded")}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving venue to database: {Message}", ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the venue. Please try again.");
                }
            }
            
            // If we get here, there was a validation error, so return the view with the model
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,Description,IsAvailable,CreatedDate")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing venue to preserve CreatedBy and handle image replacement
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    if (existingVenue == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists and is not a placeholder
                        if (!string.IsNullOrEmpty(existingVenue.ImageUrl) && !existingVenue.ImageUrl.Contains("placeholder.com"))
                        {
                            await _storageService.DeleteImageAsync(existingVenue.ImageUrl, "venues");
                        }

                        // Upload new image
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "venues");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            venue.ImageUrl = imageUrl;
                        }
                    }
                    else
                    {
                        // Keep existing image if no new image uploaded
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    venue.CreatedBy = existingVenue.CreatedBy;
                    venue.CreatedDate = existingVenue.CreatedDate;

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Venue '{venue.VenueName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Check if venue has active bookings
            if (venue.Bookings != null && venue.Bookings.Any(b => b.Status == "Confirmed" || b.Status == "Pending"))
            {
                TempData["Error"] = "Cannot delete venue with active bookings!";
                return RedirectToAction(nameof(Index));
            }

            // Check if venue has any associated PreBookings
            var hasPreBookings = await _context.PreBookings.AnyAsync(pb => pb.PreferredVenueId == id);
            if (hasPreBookings)
            {
                TempData["Error"] = "Cannot delete venue with associated pre-bookings!";
                return RedirectToAction(nameof(Index));
            }

            // Check if venue has any associated Events
            var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
            if (hasEvents)
            {
                TempData["Error"] = "Cannot delete venue with associated events!";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);
            
            if (venue != null)
            {
                // Check for active bookings
                var activeBookings = venue.Bookings?.Where(b => 
                    b.Status != "Cancelled" && 
                    b.Status != "Completed" &&
                    b.EndDate >= DateTime.Now).ToList() ?? new List<Booking>();

                if (activeBookings.Any())
                {
                    var bookingNumbers = activeBookings.Select(b => b.BookingNumber).ToList();
                    TempData["Error"] = $"Cannot delete venue '{venue.VenueName}' because it has {activeBookings.Count} active booking(s): {string.Join(", ", bookingNumbers)}. Please cancel or complete these bookings first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Check if venue has any associated PreBookings
                var hasPreBookings = await _context.PreBookings.AnyAsync(pb => pb.PreferredVenueId == id);
                if (hasPreBookings)
                {
                    TempData["Error"] = $"Cannot delete venue '{venue.VenueName}' because it has associated pre-bookings. Please remove or update these pre-bookings first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Check if venue has any associated Events
                var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
                if (hasEvents)
                {
                    TempData["Error"] = $"Cannot delete venue '{venue.VenueName}' because it has associated events. Please remove or update these events first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Venue '{venue.VenueName}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}