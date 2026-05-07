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
                // Check if database is accessible
                if (!await _context.Database.CanConnectAsync())
                {
                    _logger.LogError("Database connection failed");
                    TempData["Error"] = "Unable to connect to the database. Please check your connection settings.";
                    return View(new List<Booking>());
                }

                // Get dashboard statistics with safe defaults
                try
                {
                    ViewBag.TotalVenues = await _context.Venues.CountAsync();
                }
                catch
                {
                    ViewBag.TotalVenues = 0;
                }

                try
                {
                    ViewBag.TotalEvents = await _context.Events.CountAsync();
                }
                catch
                {
                    ViewBag.TotalEvents = 0;
                }

                try
                {
                    ViewBag.TotalBookings = await _context.Bookings.CountAsync();
                    ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
                    ViewBag.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");
                    ViewBag.TotalCustomers = await _context.Bookings.Select(b => b.CustomerPhone).Distinct().CountAsync();
                }
                catch
                {
                    ViewBag.TotalBookings = 0;
                    ViewBag.PendingBookings = 0;
                    ViewBag.ConfirmedBookings = 0;
                    ViewBag.TotalCustomers = 0;
                }

                // Get recent bookings with error handling
                List<Booking> recentBookings = new List<Booking>();
                try
                {
                    recentBookings = await _context.Bookings
                        .Include(b => b.Venue)
                        .Include(b => b.Event)
                        .Include(b => b.CreatedByAdmin)
                        .OrderByDescending(b => b.CreatedDate)
                        .Take(10)
                        .ToListAsync() ?? new List<Booking>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load recent bookings");
                }

                // Get upcoming bookings with error handling
                List<Booking> upcomingBookings = new List<Booking>();
                try
                {
                    upcomingBookings = await _context.Bookings
                        .Include(b => b.Venue)
                        .Include(b => b.Event)
                        .Where(b => b.StartDate >= DateTime.Now && b.Status != "Cancelled")
                        .OrderBy(b => b.StartDate)
                        .Take(5)
                        .ToListAsync() ?? new List<Booking>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load upcoming bookings");
                }

                // Get popular venues - always show top 3 venues even with 0 bookings
                List<Venue> popularVenues = new List<Venue>();
                try
                {
                    popularVenues = await _context.Venues
                        .OrderBy(v => v.VenueName) // Sort by name for consistency
                        .Take(3) // Take first 3 venues
                        .ToListAsync() ?? new List<Venue>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load popular venues");
                }

                ViewBag.RecentBookings = recentBookings;
                ViewBag.PopularVenues = popularVenues;

                // Check if we have any data and provide helpful message
                if (ViewBag.TotalVenues == 0 && ViewBag.TotalEvents == 0 && ViewBag.TotalBookings == 0)
                {
                    TempData["Info"] = "Welcome to EventEase! Start by adding your first venue or event to get started.";
                }

                return View(upcomingBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error loading dashboard");
                TempData["Error"] = "A critical error occurred while loading the dashboard. Please try again or contact support.";
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