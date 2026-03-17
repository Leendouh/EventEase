using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    [Authorize]
    public class PreBookingController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public PreBookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PreBooking
        public async Task<IActionResult> Index(string searchTerm, string customerName, string customerPhone,
            string? status, DateTime? fromDate, DateTime? toDate, int? preferredVenueId)
        {
            var query = _context.PreBookings
                .Include(p => p.PreferredVenue)
                .Include(p => p.CreatedByAdmin)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.EventName.Contains(searchTerm) ||
                                         p.CustomerName.Contains(searchTerm) ||
                                         p.CustomerPhone.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(p => p.CustomerName.Contains(customerName));
            }

            if (!string.IsNullOrEmpty(customerPhone))
            {
                query = query.Where(p => p.CustomerPhone.Contains(customerPhone));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.RequestDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.RequestDate <= toDate);
            }

            if (preferredVenueId.HasValue)
            {
                query = query.Where(p => p.PreferredVenueId == preferredVenueId);
            }

            var preBookings = await query
                .OrderByDescending(p => p.RequestDate)
                .ToListAsync();

            var viewModel = new PreBookingSearchViewModel
            {
                SearchTerm = searchTerm,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                PreferredVenueId = preferredVenueId,
                PreBookings = preBookings,
                StatusList = new SelectList(new[] { "Waiting", "Notified", "Converted", "Expired" }),
                VenueList = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName")
            };

            return View(viewModel);
        }

        // GET: PreBooking/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName");
            return View();
        }

        // POST: PreBooking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePreBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var preBooking = new PreBooking
                {
                    RequestDate = DateTime.Now,
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail ?? string.Empty,
                    EventName = model.EventName,
                    EventDescription = model.EventDescription ?? string.Empty,
                    ExpectedAttendees = model.ExpectedAttendees,
                    PreferredVenueId = model.PreferredVenueId,
                    PreferredStartDate = model.PreferredStartDate,
                    PreferredEndDate = model.PreferredEndDate,
                    Status = "Waiting",
                    Notes = model.Notes ?? string.Empty,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetCurrentUserId()
                };

                _context.PreBookings.Add(preBooking);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Pre-booking for '{model.EventName}' created successfully. Customer will be notified when venue becomes available.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", model.PreferredVenueId);
            return View(model);
        }

        // GET: PreBooking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preBooking = await _context.PreBookings
                .Include(p => p.PreferredVenue)
                .Include(p => p.CreatedByAdmin)
                .FirstOrDefaultAsync(p => p.PreBookingId == id);

            if (preBooking == null)
            {
                return NotFound();
            }

            // Find available venues for this date range
            List<Venue> availableVenues = new();
            if (preBooking.PreferredStartDate.HasValue && preBooking.PreferredEndDate.HasValue)
            {
                availableVenues = await _context.Venues
                    .Where(v => v.IsAvailable &&
                        !_context.Bookings.Any(b =>
                            b.VenueId == v.VenueId &&
                            b.Status != "Cancelled" &&
                            b.Status != "Completed" &&
                            (
                                (preBooking.PreferredStartDate >= b.StartDate && preBooking.PreferredStartDate < b.EndDate) ||
                                (preBooking.PreferredEndDate > b.StartDate && preBooking.PreferredEndDate <= b.EndDate)
                            )))
                    .ToListAsync();
            }

            var viewModel = new PreBookingDetailsViewModel
            {
                PreBooking = preBooking,
                AvailableVenues = availableVenues
            };

            return View(viewModel);
        }

        // POST: PreBooking/Notify/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Notify(int id)
        {
            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            preBooking.Status = "Notified";
            await _context.SaveChangesAsync();

            // Here you would typically send an email or SMS notification
            TempData["Success"] = $"Customer {preBooking.CustomerName} has been notified that venue is now available.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: PreBooking/Convert/5
        public async Task<IActionResult> Convert(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preBooking = await _context.PreBookings
                .Include(p => p.PreferredVenue)
                .FirstOrDefaultAsync(p => p.PreBookingId == id);

            if (preBooking == null)
            {
                return NotFound();
            }

            if (preBooking.Status == "Converted")
            {
                TempData["Info"] = "This pre-booking has already been converted to a booking.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Find available venues
            var availableVenues = await _context.Venues
                .Where(v => v.IsAvailable)
                .ToListAsync();

            ViewBag.Venues = new SelectList(availableVenues, "VenueId", "VenueName");
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");

            return View(preBooking);
        }

        // POST: PreBooking/Convert/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(int id, ConvertPreBookingViewModel model)
        {
            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Create actual booking
                var booking = new Booking
                {
                    BookingDate = DateTime.Now,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Status = "Confirmed",
                    Notes = $"Converted from pre-booking #{preBooking.PreBookingId}: {preBooking.Notes}",
                    CustomerName = preBooking.CustomerName,
                    CustomerPhone = preBooking.CustomerPhone,
                    CustomerEmail = preBooking.CustomerEmail,
                    EventId = model.EventId,
                    VenueId = model.VenueId,
                    PreBookingId = preBooking.PreBookingId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetCurrentUserId()
                };

                // Check venue availability
                bool isAvailable = !await _context.Bookings.AnyAsync(b =>
                    b.VenueId == model.VenueId &&
                    b.Status != "Cancelled" &&
                    b.Status != "Completed" &&
                    (
                        (model.StartDate >= b.StartDate && model.StartDate < b.EndDate) ||
                        (model.EndDate > b.StartDate && model.EndDate <= b.EndDate) ||
                        (model.StartDate <= b.StartDate && model.EndDate >= b.EndDate)
                    ));

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Selected venue is not available for the chosen dates.");
                    ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", model.VenueId);
                    ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", model.EventId);
                    return View(preBooking);
                }

                _context.Bookings.Add(booking);

                // Update pre-booking status
                preBooking.Status = "Converted";
                preBooking.ConvertedToBookingId = booking.BookingId;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Pre-booking successfully converted to booking #{booking.BookingNumber}!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", model.VenueId);
            ViewBag.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", model.EventId);
            return View(preBooking);
        }

        // POST: PreBooking/Expire/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(int id)
        {
            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            preBooking.Status = "Expired";
            await _context.SaveChangesAsync();

            TempData["Info"] = $"Pre-booking for {preBooking.CustomerName} has been marked as expired.";
            return RedirectToAction(nameof(Index));
        }
        // GET: PreBooking/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preBooking = await _context.PreBookings
                .Include(p => p.PreferredVenue)
                .Include(p => p.CreatedByAdmin)
                .FirstOrDefaultAsync(p => p.PreBookingId == id);

            if (preBooking == null)
            {
                return NotFound();
            }

            return View(preBooking);
        }

        // POST: PreBooking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            preBooking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Info"] = $"Pre-booking for {preBooking.CustomerName} has been cancelled.";
            return RedirectToAction(nameof(Index));
        }
        // GET: PreBooking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            // Don't allow editing converted pre-bookings
            if (preBooking.Status == "Converted")
            {
                TempData["Error"] = "Cannot edit a pre-booking that has already been converted to a booking.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new EditPreBookingViewModel
            {
                PreBookingId = preBooking.PreBookingId,
                CustomerName = preBooking.CustomerName,
                CustomerPhone = preBooking.CustomerPhone,
                CustomerEmail = preBooking.CustomerEmail,
                EventName = preBooking.EventName,
                EventDescription = preBooking.EventDescription,
                ExpectedAttendees = preBooking.ExpectedAttendees,
                PreferredVenueId = preBooking.PreferredVenueId,
                PreferredStartDate = preBooking.PreferredStartDate,
                PreferredEndDate = preBooking.PreferredEndDate,
                Status = preBooking.Status,
                Notes = preBooking.Notes
            };

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", preBooking.PreferredVenueId);
            return View(viewModel);
        }

        // POST: PreBooking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPreBookingViewModel model)
        {
            if (id != model.PreBookingId)
            {
                return NotFound();
            }

            var preBooking = await _context.PreBookings.FindAsync(id);
            if (preBooking == null)
            {
                return NotFound();
            }

            // Don't allow editing converted pre-bookings
            if (preBooking.Status == "Converted")
            {
                TempData["Error"] = "Cannot edit a pre-booking that has already been converted to a booking.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update properties
                    preBooking.CustomerName = model.CustomerName;
                    preBooking.CustomerPhone = model.CustomerPhone;
                    preBooking.CustomerEmail = model.CustomerEmail;
                    preBooking.EventName = model.EventName;
                    preBooking.EventDescription = model.EventDescription;
                    preBooking.ExpectedAttendees = model.ExpectedAttendees;
                    preBooking.PreferredVenueId = model.PreferredVenueId;
                    preBooking.PreferredStartDate = model.PreferredStartDate;
                    preBooking.PreferredEndDate = model.PreferredEndDate;
                    preBooking.Status = model.Status;
                    preBooking.Notes = model.Notes;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Pre-booking for {preBooking.CustomerName} updated successfully!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PreBookingExists(preBooking.PreBookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", model.PreferredVenueId);
            return View(model);
        }

        private bool PreBookingExists(int id)
        {
            return _context.PreBookings.Any(e => e.PreBookingId == id);
        }
    }
}