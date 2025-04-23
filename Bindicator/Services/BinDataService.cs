using Bindicator.Data;
using Bindicator.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Services
{
    /// <summary>
    /// Service to handle operations related to bin data. Gets the latest bin readings
    /// Maps to BinStatusViewModel
    /// </summary>
    public class BinDataService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinDataService"/> class.
        /// </summary>
        /// <param name="context">The database context to be used.</param>
        public BinDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the latest bin statuses asynchronously.
        /// </summary>
        /// <returns>A list of the latest <see cref="BinStatusViewModel"/>.</returns>
        public async Task<List<BinStatusViewModel>> GetLatestBinStatusesAsync()
        {
            var latestSensorData = await _context.SensorReadings
                .GroupBy(b => new { b.Postcode, b.Street, b.BinNumber })
                .Select(g => g.OrderByDescending(r => r.Timestamp).First())
                .ToListAsync();

            var latestEnvData = await _context.EnvironmentReadings
                .GroupBy(e => new { e.Postcode, e.Street, e.BinNumber })
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToListAsync();

            var viewModels = from sensor in latestSensorData
                             join env in latestEnvData
                             on new { sensor.Postcode, sensor.Street, sensor.BinNumber }
                             equals new { env.Postcode, env.Street, env.BinNumber } into joined
                             from env in joined.DefaultIfEmpty()
                             select new BinStatusViewModel
                             {
                                 Postcode = sensor.Postcode,
                                 Street = sensor.Street,
                                 BinNumber = sensor.BinNumber,
                                 FillLevel = sensor.FillLevel,
                                 Weight = sensor.Weight,
                                 Density = sensor.Density,
                                 Timestamp = sensor.Timestamp,
                                 Temperature = env?.Temperature,
                                 Humidity = env?.Humidity,
                                 Latitude = sensor.Latitude,
                                 Longitude = sensor.Longitude
                             };

            return viewModels.ToList();
        }
    }
}