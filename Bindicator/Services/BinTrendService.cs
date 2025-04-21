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

        /// <summary>
        /// Initializes a new instance of the <see cref="BinTrendService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public BinTrendService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the trend of bin fill levels and detects spikes.
        /// </summary>
        /// <param name="postcode">The postcode of the bin location.</param>
        /// <param name="street">The street of the bin location.</param>
        /// <param name="binNumber">The bin number.</param>
        /// <returns>A <see cref="BinTrendViewModel"/> containing the trend data and detected spikes.</returns>
        public async Task<BinTrendViewModel> GetTrendAsync(string postcode, string street, int binNumber)
        {
            var readings = await _context.SensorReadings
                .Where(b => b.Postcode == postcode && b.Street == street && b.BinNumber == binNumber)
                .OrderBy(b => b.Timestamp)
                .ToListAsync();

            var spikes = new List<SpikePoint>();

            for (int i = 1; i < readings.Count; i++)
            {
                var prev = readings[i - 1];
                var current = readings[i];
                if ((current.FillLevel - prev.FillLevel) >= 30)
                {
                    spikes.Add(new SpikePoint
                    {
                        Timestamp = current.Timestamp,
                        FromLevel = prev.FillLevel,
                        ToLevel = current.FillLevel
                    });
                }
            }

            // Simple linear regression prediction based on weight
            DateTime? predictedDate = null;
            double? daysToFull = null;

            if (readings.Count >= 2)
            {
                var x = readings.Select(r => (r.Timestamp - readings[0].Timestamp).TotalDays).ToArray();
                var y = readings.Select(r => (double)r.Weight).ToArray();

                var n = x.Length;
                var xAvg = x.Average();
                var yAvg = y.Average();

                var numerator = x.Zip(y, (xi, yi) => (xi - xAvg) * (yi - yAvg)).Sum();
                var denominator = x.Sum(xi => Math.Pow(xi - xAvg, 2));

                if (denominator != 0)
                {
                    var slope = numerator / denominator;
                    var intercept = yAvg - slope * xAvg;

                    const double maxWeight = 25.0; // max weight before full
                    daysToFull = (maxWeight - intercept) / slope;

                    if (daysToFull > 0)
                        predictedDate = readings[0].Timestamp.AddDays(daysToFull.Value);
                }
            }

            return new BinTrendViewModel
            {
                Postcode = postcode,
                Street = street,
                BinNumber = binNumber,
                Readings = readings,
                Spikes = spikes,
                PredictedFullDate = predictedDate,
                DaysToFull = daysToFull
            };
        }
    }
}