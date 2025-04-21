using Bindicator.Data;
using Bindicator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Controllers
{
    /// <summary>
    /// Controller for handling dashboard-related actions.
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly BinDataService _binData;
        private readonly BinTrendService _binTrend;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="binData">The service for bin data operations.</param>
        /// <param name="binTrend">The service for bin trend operations.</param>
        /// <param name="context">The application database context.</param>
        public DashboardController(BinDataService binData, BinTrendService binTrend, ApplicationDbContext context)
        {
            _binData = binData;
            _binTrend = binTrend;
            _context = context;
        }

        /// <summary>
        /// Displays the dashboard index view with the latest bin statuses.
        /// </summary>
        /// <returns>The dashboard index view.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _binData.GetLatestBinStatusesAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // You could log this error to the console, file, or a logger
                ViewBag.ErrorMessage = "🚫 Unable to load data. Please check your database connection.";
                return View("NoData");
            }
        }


        /// <summary>
        /// Displays the trend view for a specific bin.
        /// </summary>
        /// <param name="postcode">The postcode of the bin location.</param>
        /// <param name="street">The street of the bin location.</param>
        /// <param name="binNumber">The bin number.</param>
        /// <returns>The trend view for the specified bin.</returns>
        public async Task<IActionResult> Trend(string postcode, string street, int binNumber)
        {
            var viewModel = await _binTrend.GetTrendAsync(postcode, street, binNumber);
            return View(viewModel);
        }

        /// <summary>
        /// Gets the latest bin statuses and returns a partial view.
        /// </summary>
        /// <returns>A partial view with the latest bin statuses.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLatestBins()
        {
            var viewModel = await _binData.GetLatestBinStatusesAsync();
            return PartialView("_BinTable", viewModel);
        }

        /// <summary>
        /// Displays the map view with the latest sensor readings for each bin.
        /// </summary>
        /// <returns>The map view with the latest sensor readings.</returns>
        public async Task<IActionResult> Map()
        {
            var bins = await _context.SensorReadings
                .GroupBy(b => new { b.Postcode, b.Street, b.BinNumber, b.Latitude, b.Longitude })
                .Select(g => g.OrderByDescending(b => b.Timestamp).First())
                .ToListAsync();

            return View(bins);
        }

        // Seed data
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            await DbSeeder.SeedAsync(_context);
            return RedirectToAction("Index"); // or return Json if you're using AJAX
        }

    }
}