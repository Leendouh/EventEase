using System.Diagnostics;
using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get dashboard statistics
                ViewBag.TotalVenues = await _context.Venues.CountAsync();
                ViewBag.TotalEvents = await _context.Events.CountAsync();
                ViewBag.TotalBookings = await _context.Bookings.CountAsync();
                ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
                ViewBag.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");
                ViewBag.TotalCustomers = await _context.Bookings.Select(b => b.CustomerPhone).Distinct().CountAsync();

                // Get recent bookings
                var recentBookings = await _context.Bookings
                    .Include(b => b.Venue)
                    .Include(b => b.Event)
                    .Include(b => b.CreatedByAdmin)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(10)
                    .ToListAsync();

                // Get upcoming bookings
                var upcomingBookings = await _context.Bookings
                    .Include(b => b.Venue)
                    .Include(b => b.Event)
                    .Where(b => b.StartDate >= DateTime.Now && b.Status != "Cancelled")
                    .OrderBy(b => b.StartDate)
                    .Take(5)
                    .ToListAsync();

                // Get popular venues
                var popularVenues = await _context.Venues
                    .Select(v => new
                    {
                        Venue = v,
                        BookingCount = v.Bookings.Count()
                    })
                    .OrderByDescending(x => x.BookingCount)
                    .Take(3)
                    .Select(x => x.Venue)
                    .ToListAsync();

                ViewBag.RecentBookings = recentBookings;
                ViewBag.PopularVenues = popularVenues;

                return View(upcomingBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Unable to load dashboard data.";
                return View(new List<Booking>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}