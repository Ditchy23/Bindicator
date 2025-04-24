using Bindicator.Data;
using Bindicator.Models;
using Bindicator.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Services
{
    /// <summary>
    /// Service to handle bin trend operations. Fetches readings for a specific bin
    /// Handles the logic for detecting spikes in fill levels
    /// </summary>
    public class BinTrendService
    {
        private readonly ApplicationDbContext _context;

        public BinTrendService(ApplicationDbContext context)
        {
            _context = context;
        }

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
                warnings.Add("⚠️ Warning: Bin is nearly overloaded. Consider early collection.");

            if (sensor != null && sensor.FillLevel > 85)
                warnings.Add("🚨 Notice: Bin is nearly full.");

            if (env != null && env.Temperature < 0)
                warnings.Add("❄️ Notice: Freezing detected. Check for blockages.");

            if (env != null && env.Humidity > 90)
                warnings.Add("💧 Notice: High humidity detected in bin.");

            return warnings;
        }

        /// <summary>
        /// Gets the trend of bin fill levels, detects spikes, and builds warning history.
        /// </summary>
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

            // Simple linear regression prediction based on weight
            //DateTime? predictedDate = null;
            //double? daysToFull = null;

            //if (readings.Count >= 2)
            //{
            //    var x = readings.Select(r => (r.Timestamp - readings[0].Timestamp).TotalDays).ToArray();
            //    var y = readings.Select(r => (double)r.Weight).ToArray();

            //    var xAvg = x.Average();
            //    var yAvg = y.Average();

            //    var numerator = x.Zip(y, (xi, yi) => (xi - xAvg) * (yi - yAvg)).Sum();
            //    var denominator = x.Sum(xi => Math.Pow(xi - xAvg, 2));

            //    if (denominator != 0)
            //    {
            //        var slope = numerator / denominator;
            //        var intercept = yAvg - slope * xAvg;

            //        const double maxWeight = 25.0; // max weight before full
            //        daysToFull = (maxWeight - intercept) / slope;

            //        if (daysToFull > 0)
            //            predictedDate = readings[0].Timestamp.AddDays(daysToFull.Value);
            //    }
            //}

            // --- Build warning history ---
            var warningHistory = new List<WarningEntry>();

            // For each sensor reading, find the most recent env reading at/before that time
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

            // Also check env readings not covered by a sensor reading
            foreach (var env in envReadings)
            {
                // If no warning for this timestamp (from above), check env-only warnings
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

            return new BinTrendViewModel
            {
                Postcode = postcode,
                Street = street,
                BinNumber = binNumber,
                Readings = readings,
                EnvironmentReadings = envReadings,
                //PredictedFullDate = predictedDate,
                //DaysToFull = daysToFull,
                WarningHistory = warningHistory
            };
        }
    }
}
