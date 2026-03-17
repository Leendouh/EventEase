using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenueController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,ImageUrl,Description,IsAvailable")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                venue.CreatedDate = DateTime.Now;
                venue.CreatedBy = GetCurrentUserId();

                _context.Add(venue);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Venue '{venue.VenueName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
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
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl,Description,IsAvailable,CreatedDate")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original CreatedBy
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    venue.CreatedBy = existingVenue?.CreatedBy;
                    venue.CreatedDate = existingVenue?.CreatedDate ?? DateTime.Now;

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
                .FirstOrDefaultAsync(v => v.VenueId == id);

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

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
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