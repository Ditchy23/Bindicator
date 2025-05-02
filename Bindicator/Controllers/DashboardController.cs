using Bindicator.Data;
using Bindicator.Helpers;
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
            catch (Exception)
            {
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
        /// Retrieves the latest trend data for a specific bin in JSON format.
        /// </summary>
        /// <param name="postcode">The postcode of the bin location.</param>
        /// <param name="street">The street of the bin location.</param>
        /// <param name="binNumber">The bin number.</param>
        /// <returns>A JSON object containing the latest trend data.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLatestTrendData(string postcode, string street, int binNumber)
        {
            var viewModel = await _binTrend.GetTrendAsync(postcode, street, binNumber);

            return Json(new
            {
                readings = viewModel.Readings.Select(r => new
                {
                    timestamp = r.Timestamp,
                    fillLevel = r.FillLevel,
                    weight = r.Weight,
                    density = r.Density
                }).ToList(),

                environmentReadings = viewModel.EnvironmentReadings.Select(e => new
                {
                    timestamp = e.Timestamp,
                    temperature = e.Temperature,
                    humidity = e.Humidity
                }).ToList()
            });
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
            try
            {
                var bins = await _context.SensorReadings
                    .GroupBy(b => new { b.Postcode, b.Street, b.BinNumber, b.Latitude, b.Longitude })
                    .Select(g => g.OrderByDescending(b => b.Timestamp).First())
                    .ToListAsync();

                var viewModel = bins.Select(b =>
                {
                    var sensorDataViewModel = new Bindicator.ViewModels.SensorDataViewModel
                    {
                        BinNumber = b.BinNumber,
                        Latitude = b.Latitude,
                        Longitude = b.Longitude,
                        FillLevel = b.FillLevel,
                        Weight = b.Weight,
                        Timestamp = b.Timestamp,
                        Postcode = b.Postcode,
                        Street = b.Street
                    };

                    // Pull all readings for this bin for prediction
                    var allReadings = _context.SensorReadings
                        .Where(r => r.Postcode == b.Postcode && r.Street == b.Street && r.BinNumber == b.BinNumber)
                        .OrderBy(r => r.Timestamp)
                        .ToList();

                    PredictionHelper.CalculatePredictedFullDate(allReadings, sensorDataViewModel);

                    return sensorDataViewModel;
                }).ToList();

                // Current Date to compare against predicted fullness dates
                var currentDate = DateTime.UtcNow;

                // First Collection (Priority) - Full or predicted to be full within 1 day
                var binsForFirstCollection = viewModel.Where(bin =>
                    bin.FillLevel == 100 ||
                    (bin.PredictedFullDate.HasValue && bin.PredictedFullDate.Value <= currentDate.AddDays(1))
                ).ToList();

                // Second Collection - Predicted to be full within the next 2 weeks, but not full yet
                var binsForSecondCollection = viewModel.Where(bin =>
                    (bin.FillLevel < 100 &&
                    bin.PredictedFullDate.HasValue &&
                    bin.PredictedFullDate.Value > currentDate.AddDays(1) &&
                    bin.PredictedFullDate.Value <= currentDate.AddDays(14))
                ).ToList();

                // Determine the collection dates based on the latest predicted full date in each list
                DateTime firstCollectionDate = binsForFirstCollection.Max(bin => bin.PredictedFullDate) ?? DateTime.UtcNow;
                DateTime secondCollectionDate = binsForSecondCollection.Max(bin => bin.PredictedFullDate) ?? DateTime.UtcNow.AddDays(14);

                // Calculate total weight and wagons required for each collection date
                double totalWeightFirstCollection = binsForFirstCollection.Sum(bin => bin.Weight);
                double totalWeightSecondCollection = binsForSecondCollection.Sum(bin => bin.Weight);

                int wagonsForFirstCollection = (int)Math.Ceiling(totalWeightFirstCollection / 200); // Assuming 200kg per wagon
                int wagonsForSecondCollection = (int)Math.Ceiling(totalWeightSecondCollection / 200);

                // Pass data to the view
                ViewBag.FirstCollectionDate = firstCollectionDate;
                ViewBag.SecondCollectionDate = secondCollectionDate;
                ViewBag.TotalWeightFirstCollection = totalWeightFirstCollection;
                ViewBag.TotalWeightSecondCollection = totalWeightSecondCollection;
                ViewBag.WagonsForFirstCollection = wagonsForFirstCollection;
                ViewBag.WagonsForSecondCollection = wagonsForSecondCollection;
                ViewBag.BinsForFirstCollection = binsForFirstCollection;
                ViewBag.BinsForSecondCollection = binsForSecondCollection;
                ViewBag.AllBins = viewModel;
                return View(viewModel);

            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "🚫 Unable to load data. Please check your database connection.";
                return View("NoData");
            }
        }

        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            await DbSeeder.SeedAsync(_context);
            return RedirectToAction("Index");
        }
    }
}