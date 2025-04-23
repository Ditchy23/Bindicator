using Bindicator.Data;
using Bindicator.Services;
using Bindicator.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Controllers
{
    /// <summary>
    /// Controller for handling dashboard-related actions.
    /// This includes displaying the dashboard, trends, and maps.
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

        /// <summary>
        /// Displays the edit location view for a specific bin.
        /// </summary>
        /// <param name="postcode"></param>
        /// <param name="street"></param>
        /// <param name="binNumber"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditLocation(string postcode, string street, int binNumber)
        {
            // Get latest reading
            var reading = await _context.SensorReadings
                .Where(b => b.Postcode == postcode && b.Street == street && b.BinNumber == binNumber)
                .OrderByDescending(b => b.Timestamp)
                .FirstOrDefaultAsync();

            if (reading == null) return NotFound();

            var viewModel = new EditLocationViewModel
            {
                Postcode = postcode,
                Street = street,
                BinNumber = binNumber,
                Latitude = reading.Latitude,
                Longitude = reading.Longitude
            };

            return View(viewModel);
        }

        /// <summary>
        /// Updates the location of a bin based on the provided model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditLocation(EditLocationViewModel model)
        {
            // Update all readings (or just latest) for this bin
            var readings = await _context.SensorReadings
                .Where(b => b.Postcode == model.Postcode && b.Street == model.Street && b.BinNumber == model.BinNumber)
                .ToListAsync();

            foreach (var r in readings)
            {
                r.Latitude = model.Latitude;
                r.Longitude = model.Longitude;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Map");
        }

        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            await DbSeeder.SeedAsync(_context);
            return RedirectToAction("Index"); // or return Json if you're using AJAX
        }
    }
}