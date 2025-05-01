using Bindicator.Models;
using Microsoft.EntityFrameworkCore;

namespace Bindicator.Data
{
    /// <summary>
    /// Represents the application's database context, providing access to the database tables.
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
        /// Gets or sets the database table for sensor readings.
        /// </summary>
        public DbSet<SensorData> SensorReadings { get; set; }

        /// <summary>
        /// Gets or sets the database table for environment readings.
        /// </summary>
        public DbSet<EnvironmentData> EnvironmentReadings { get; set; }

        /// <summary>
        /// Gets or sets the database table for sensor analysis data.
        /// </summary>
        public DbSet<SensorAnalysisData> SensorAnalysisReadings { get; set; }
    }
}