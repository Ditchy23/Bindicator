using Bindicator.Data;
using Bindicator.Helpers;
using Bindicator.Models;
using Bindicator.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Services
{
    /// <summary>
    /// Service to handle bin trend operations. Fetches readings for a specific bin,
    /// calculates predictions, and builds warning history.
    /// </summary>
    public class BinTrendService
    {
        private readonly ApplicationDbContext _context;

        public BinTrendService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates the predicted date when the bin will be full based on the recent trend of fill levels.
        /// </summary>
        /// <param name="model">The view model containing bin readings and other related data.</param>
        //public void CalculatePredictedFullDate(BinTrendViewModel model)
        //{
        //    var readings = model.Readings;
        //    if (readings.Count >= 3)
        //    {
        //        // Use last 3 readings for a recent trend
        //        var recent = readings.Skip(Math.Max(0, readings.Count - 3)).ToList();

        //        // Ensure all readings are increasing
        //        if (recent[0].FillLevel < recent[1].FillLevel && recent[1].FillLevel < recent[2].FillLevel)
        //        {
        //            var first = recent[0];
        //            var last = recent[2];
        //            double deltaFill = last.FillLevel - first.FillLevel;
        //            double days = (last.Timestamp - first.Timestamp).TotalDays;

        //            if (deltaFill > 0 && days > 0)
        //            {
        //                double ratePerDay = deltaFill / days;
        //                double remaining = 100.0 - last.FillLevel;
        //                double daysToFull = remaining / ratePerDay;
        //                // Clamp to max 21 days for demo
        //                daysToFull = Math.Min(daysToFull, 21);

        //                model.PredictedFullDate = last.Timestamp.AddDays(daysToFull);
        //                model.DaysToFull = daysToFull;
        //                return;
        //            }
        //        }
        //    }
        //    model.PredictedFullDate = null;
        //    model.DaysToFull = null;
        //}

        /// <summary>
        /// Helper to get warning messages for a sensor and environment reading.
        /// </summary>
        private List<string> GetWarnings(SensorData? sensor, EnvironmentData? env)
        {
            var warnings = new List<string>();

            if (env != null && env.Temperature > 40 && env.Humidity < 25)
                warnings.Add("🔥 Warning: High temperature and low humidity detected. Fire risk!");

            if (env != null && env.Temperature > 20 && env.Temperature < 35 && env.Humidity > 70)
                warnings.Add("🦠 Warning: Warm and humid conditions. Increased risk of bacteria/mold.");

            if (sensor != null && sensor.Weight > 22)
                warnings.Add("⚠️ Warning: Bin is nearly overloaded.");

            if (sensor != null && sensor.FillLevel > 85)
                warnings.Add("🚨 Notice: Bin is nearly full.");

            if (env != null && env.Temperature < 0)
                warnings.Add("❄️ Notice: Freezing detected. Potential dangerous conditions.");

            if (env != null && env.Humidity > 90)
                warnings.Add("💧 Notice: High humidity detected in bin.");

            return warnings;
        }

        ///<summary>
        /// Retrieves the trend data for a specific bin, including sensor readings, environment readings, and warning history.
        /// </summary>
        /// <param name="postcode">The postcode of the bin's location.</param>
        /// <param name="street">The street of the bin's location.</param>
        /// <param name="binNumber">The bin number to retrieve data for.</param>
        /// <returns>A <see cref="BinTrendViewModel"/> containing the bin's trend data, including readings, warnings, and predicted full date.</returns>
        /// <remarks>
        /// This method fetches sensor and environment readings from the database, calculates warnings based on the readings,
        /// and predicts when the bin will be full based on recent trends.
        /// </remarks>
        public async Task<BinTrendViewModel> GetTrendAsync(string postcode, string street, int binNumber)
        {
            var readings = await _context.SensorReadings
                .Where(b => b.Postcode == postcode && b.Street == street && b.BinNumber == binNumber)
                .OrderBy(b => b.Timestamp)
                .ToListAsync();

            var envReadings = await _context.EnvironmentReadings
                .Where(e => e.Postcode == postcode && e.Street == street && e.BinNumber == binNumber)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();

            // Build warning history
            var warningHistory = new List<WarningEntry>();
            foreach (var sensor in readings)
            {
                var env = envReadings.LastOrDefault(e => e.Timestamp <= sensor.Timestamp);
                var warnings = GetWarnings(sensor, env);
                foreach (var msg in warnings)
                {
                    warningHistory.Add(new WarningEntry
                    {
                        Timestamp = sensor.Timestamp,
                        Message = msg
                    });
                }
            }

            foreach (var env in envReadings)
            {
                if (!warningHistory.Any(w => w.Timestamp == env.Timestamp))
                {
                    var warnings = GetWarnings(null, env);
                    foreach (var msg in warnings)
                    {
                        warningHistory.Add(new WarningEntry
                        {
                            Timestamp = env.Timestamp,
                            Message = msg
                        });
                    }
                }
            }

            var viewModel = new BinTrendViewModel
            {
                Postcode = postcode,
                Street = street,
                BinNumber = binNumber,
                Readings = readings,
                EnvironmentReadings = envReadings,
                WarningHistory = warningHistory
            };

            // Use the helper method to calculate the predicted full date
            PredictionHelper.CalculatePredictedFullDate(readings, viewModel);

            return viewModel;
        }
    }
}