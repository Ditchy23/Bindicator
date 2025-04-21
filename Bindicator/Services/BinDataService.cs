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
            var allReadings = await _context.SensorReadings
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();

            var latest = allReadings
                .GroupBy(b => new { b.Postcode, b.Street, b.BinNumber })
                .Select(g => g.First())
                .OrderByDescending(b => b.Timestamp)
                .ToList();

            return latest.Select(b => new BinStatusViewModel
            {
                Postcode = b.Postcode,
                Street = b.Street,
                BinNumber = b.BinNumber,
                FillLevel = b.FillLevel,
                Weight = b.Weight,
                Density = b.Density,
                Timestamp = b.Timestamp,
                Latitude = b.Latitude,
                Longitude = b.Longitude
            }).ToList();
        }
    }
}