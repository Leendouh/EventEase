using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService _storageService;

        public EventController(ApplicationDbContext context, IAzureStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        // GET: Event with Search
        public async Task<IActionResult> Index(string searchTerm, DateTime? startDate, DateTime? endDate, int? venueId, bool? showUnconfirmed)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.EventName.Contains(searchTerm) ||
                                         e.Description.Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EventDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EventDate <= endDate);
            }

            if (venueId.HasValue)
            {
                query = query.Where(e => e.VenueId == venueId);
            }

            if (showUnconfirmed.HasValue && showUnconfirmed.Value)
            {
                query = query.Where(e => !e.IsVenueConfirmed);
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            var viewModel = new EventSearchViewModel
            {
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate,
                VenueId = venueId,
                ShowUnconfirmed = showUnconfirmed,
                Events = events,
                VenueList = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName")
            };

            return View(viewModel);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Event/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName");
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,ExpectedAttendees,VenueId")] Event @event, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "event-images");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            @event.ImageUrl = imageUrl;
                        }
                        else
                        {
                            @event.ImageUrl = "https://via.placeholder.com/300x200?text=Event+Image";
                        }
                    }
                    else
                    {
                        @event.ImageUrl = "https://via.placeholder.com/300x200?text=Event+Image";
                    }

                    @event.CreatedDate = DateTime.Now;
                    @event.CreatedBy = GetCurrentUserId();
                    @event.IsVenueConfirmed = @event.VenueId.HasValue;

                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Event '{@event.EventName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the event. Please try again.");
                }
            }

            ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,ExpectedAttendees,VenueId,IsVenueConfirmed,CreatedDate")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing event to preserve CreatedBy and handle image replacement
                    var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists and is not a placeholder
                        if (!string.IsNullOrEmpty(existingEvent.ImageUrl) && !existingEvent.ImageUrl.Contains("placeholder.com"))
                        {
                            await _storageService.DeleteImageAsync(existingEvent.ImageUrl, "event-images");
                        }

                        // Upload new image
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "event-images");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            @event.ImageUrl = imageUrl;
                        }
                    }
                    else
                    {
                        // Keep existing image if no new image uploaded
                        @event.ImageUrl = existingEvent.ImageUrl;
                    }

                    @event.CreatedBy = existingEvent.CreatedBy;
                    @event.CreatedDate = existingEvent.CreatedDate;
                    @event.IsVenueConfirmed = @event.VenueId.HasValue;

                    _context.Update(@event);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Event '{@event.EventName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
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

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            // Check if event has bookings
            if (@event.Bookings != null && @event.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete event with existing bookings!";
                return RedirectToAction(nameof(Index));
            }

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);
            
            if (@event != null)
            {
                // Check for active bookings
                var activeBookings = @event.Bookings?.Where(b => 
                    b.Status != "Cancelled" && 
                    b.Status != "Completed" &&
                    b.EndDate >= DateTime.Now).ToList() ?? new List<Booking>();

                if (activeBookings.Any())
                {
                    var bookingNumbers = activeBookings.Select(b => b.BookingNumber).ToList();
                    TempData["Error"] = $"Cannot delete event '{@event.EventName}' because it has {activeBookings.Count} active booking(s): {string.Join(", ", bookingNumbers)}. Please cancel or complete these bookings first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Event '{@event.EventName}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}