using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking with Search
        public async Task<IActionResult> Index(string searchTerm, string customerName, string customerPhone,
            DateTime? startDate, DateTime? endDate, string? status, int? venueId, int? eventId)
        {
            var query = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.CreatedByAdmin)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.BookingNumber.Contains(searchTerm) ||
                                         b.CustomerName.Contains(searchTerm) ||
                                         b.CustomerPhone.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(b => b.CustomerName.Contains(customerName) ||
                                         b.CustomerSurname.Contains(customerName));
            }

            if (!string.IsNullOrEmpty(customerPhone))
            {
                query = query.Where(b => b.CustomerPhone.Contains(customerPhone));
            }

            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndDate <= endDate);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            if (venueId.HasValue)
            {
                query = query.Where(b => b.VenueId == venueId);
            }

            if (eventId.HasValue)
            {
                query = query.Where(b => b.EventId == eventId);
            }

            var bookings = await query.OrderByDescending(b => b.StartDate).ToListAsync();

            var viewModel = new BookingSearchViewModel
            {
                SearchTerm = searchTerm,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                VenueId = venueId,
                EventId = eventId,
                Bookings = bookings,
                VenueList = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName"),
                EventList = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName")
            };

            return View(viewModel);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.CreatedByAdmin)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Booking/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load events with error handling
                var events = await _context.Events
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();

                // Load available venues with error handling
                var venues = await _context.Venues
                    .Where(v => v.IsAvailable)
                    .OrderBy(v => v.VenueName)
                    .ToListAsync();

                // Log what we found (for debugging)
                Console.WriteLine($"Events found: {events?.Count ?? 0}");
                Console.WriteLine($"Venues found: {venues?.Count ?? 0}");

                // Create SelectLists - use empty lists if null
                ViewBag.Events = new SelectList(events ?? new List<Event>(), "EventId", "EventName");
                ViewBag.Venues = new SelectList(venues ?? new List<Venue>(), "VenueId", "VenueName");

                return View(new Booking());
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"ERROR in Create GET: {ex.Message}");

                // Provide empty SelectLists to prevent null reference
                ViewBag.Events = new SelectList(new List<Event>(), "EventId", "EventName");
                ViewBag.Venues = new SelectList(new List<Venue>(), "VenueId", "VenueName");

                TempData["Error"] = "Unable to load form data. Please try again.";
                return View(new Booking());
            }
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                // ALWAYS repopulate dropdowns first thing
                var events = await _context.Events.OrderBy(e => e.EventDate).ToListAsync();
                var venues = await _context.Venues.Where(v => v.IsAvailable).ToListAsync();

                ViewBag.Events = new SelectList(events ?? new List<Event>(), "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(venues ?? new List<Venue>(), "VenueId", "VenueName", booking.VenueId);

                // Server-side date validation
                if (booking.StartDate < DateTime.Now)
                {
                    ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
                }

                if (booking.EndDate <= booking.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }

                if (!ModelState.IsValid)
                {
                    return View(booking);
                }

                // Check venue availability
                bool isAvailable = !await _context.Bookings.AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.Status != "Cancelled" &&
                    b.Status != "Completed" &&
                    (
                        (booking.StartDate >= b.StartDate && booking.StartDate < b.EndDate) ||
                        (booking.EndDate > b.StartDate && booking.EndDate <= b.EndDate) ||
                        (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate)
                    ));

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "This venue is not available for the selected dates.");
                    return View(booking);
                }

                // Set audit fields
                booking.BookingDate = DateTime.Now;
                booking.CreatedDate = DateTime.Now;
                booking.CreatedBy = GetCurrentUserId();
                booking.Status = "Confirmed";

                _context.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Booking {booking.BookingNumber} created successfully for {booking.CustomerName}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Create POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the booking.");
                return View(booking);
            }
        }
        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Check venue availability (excluding current booking)
            bool isAvailable = !await _context.Bookings.AnyAsync(b =>
                b.BookingId != id &&
                b.VenueId == booking.VenueId &&
                b.Status != "Cancelled" &&
                b.Status != "Completed" &&
                (
                    (booking.StartDate >= b.StartDate && booking.StartDate < b.EndDate) ||
                    (booking.EndDate > b.StartDate && booking.EndDate <= b.EndDate) ||
                    (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate)
                ));

            if (!isAvailable)
            {
                ModelState.AddModelError("", "This venue is not available for the selected dates.");
                ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original values
                    var existingBooking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.BookingId == id);
                    booking.CreatedBy = existingBooking?.CreatedBy;
                    booking.CreatedDate = existingBooking?.CreatedDate ?? DateTime.Now;
                    booking.BookingNumber = existingBooking?.BookingNumber ?? string.Empty;
                    booking.BookingDate = existingBooking?.BookingDate ?? DateTime.Now;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Booking {booking.BookingNumber} updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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

            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking {booking.BookingNumber} cancelled successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/ByCustomer
        public async Task<IActionResult> ByCustomer(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction(nameof(Index));
            }

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Where(b => b.CustomerPhone.Contains(phone))
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            ViewBag.CustomerPhone = phone;
            return View(bookings);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}