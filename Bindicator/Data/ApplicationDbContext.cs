using Bindicator.Models;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Data
{
    /// <summary>
    /// Represents the application's database context.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the sensor readings.
        /// </summary>
        public DbSet<SensorData> SensorReadings { get; set; }

        /// <summary>
        /// Gets or sets the environment readings.
        /// </summary>
        public DbSet<EnvironmentData> EnvironmentReadings { get; set; }
    }
}
