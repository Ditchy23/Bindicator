using Bindicator.Data;
using Bindicator.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

/// <summary>
/// Class responsible for seeding the database with initial data.
/// </summary>
public class DbSeeder
{
    /// <summary>
    /// Seeds the database with sensor and environment data.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.SensorReadings.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // Coordinates for postcodes
        var postcodeCoords = new Dictionary<string, (double Lat, double Lon)>
        {
            { "TS16", (54.525079, -1.3649298) },
            { "TS17", (54.5313629, -1.2914754) },
            { "TS18", (54.5529822, -1.3193432) }
        };

        // Street names for each postcode
        var streetNames = new Dictionary<string, string[]>
        {
            { "TS16", new[] { "Formby Walk", "Alder Crescent", "Beech Road", "Maple Avenue", "Sycamore Street", "Poplar Drive", "Willow Close", "Hawthorn Way", "Elm Court", "Rowan View" } },
            { "TS17", new[] { "Oakwood Drive", "Birch Lane", "Hazel Grove", "Chestnut Place", "Spruce Gardens", "Ash Terrace", "Cedar Lane", "Pine Avenue", "Lime Crescent", "Fir Walk" } },
            { "TS18", new[] { "Cedar Avenue", "Holly Drive", "Ivy Road", "Juniper Close", "Laurel Street", "Magnolia Place", "Olive Court", "Palm Avenue", "Quince Grove", "Sycamore Walk" } }
        };

        var random = new Random(1234);
        var sensorData = new List<SensorData>();
        var environmentData = new List<EnvironmentData>();

        foreach (var postcode in postcodeCoords.Keys)
        {
            var (centerLat, centerLon) = postcodeCoords[postcode];
            var streets = streetNames[postcode];

            for (int binIdx = 0; binIdx < 10; binIdx++)
            {
                string street = streets[binIdx % streets.Length];
                int binNumber = binIdx + 1;

                double latOffset = (random.NextDouble() - 0.5) * 0.012;
                double lonOffset = (random.NextDouble() - 0.5) * 0.02;
                double binLat = centerLat + latOffset;
                double binLon = centerLon + lonOffset;

                int groupOffset = postcode switch
                {
                    "TS16" => 0,
                    "TS17" => 2,
                    "TS18" => 4,
                    _ => 0
                };

                int binOffset = (binIdx % 4);

                float fill = (float)Math.Round((double)random.Next(10, 30), 2);
                float weight = (float)Math.Round((double)random.Next(4, 10), 2);

                for (int day = 27; day >= 0; day--)
                {
                    var timestamp = now.AddDays(-day);
                    int daysSinceFirst = 27 - day;

                    bool isCollectionDay = ((daysSinceFirst - groupOffset - binOffset) % 14 == 0);

                    if (isCollectionDay && daysSinceFirst > 0)
                    {
                        fill = (float)Math.Round((double)random.Next(5, 18), 2);
                        weight = (float)Math.Round((double)random.Next(2, 7), 2);
                    }
                    else
                    {
                        if (daysSinceFirst >= 24)
                        {
                            fill = (float)Math.Round((double)Math.Min(fill + (float)(random.NextDouble() * 7.5 + 2.5), 100), 2);
                            weight = (float)Math.Round((double)Math.Min(weight + (float)(random.NextDouble() * 2 + 0.8), 25), 2);
                        }
                        else
                        {
                            fill = (float)Math.Round((double)Math.Min(fill + (float)(random.NextDouble() * 3 + 1), 100), 2);
                            weight = (float)Math.Round((double)Math.Min(weight + (float)(random.NextDouble() * 1.5 + 0.5), 25), 2);
                        }
                    }

                    float density = (float)Math.Round((double)(random.NextDouble() * 1.2 + 0.7), 2);

                    if (random.NextDouble() < 0.07)
                        fill = (float)Math.Round((double)random.Next(85, 100), 2);

                    sensorData.Add(new SensorData
                    {
                        Postcode = postcode,
                        Street = street,
                        BinNumber = binNumber,
                        FillLevel = fill,
                        Weight = weight,
                        Density = density,
                        Timestamp = timestamp,
                        Latitude = binLat,
                        Longitude = binLon
                    });

                    float temp = (float)Math.Round((double)(random.NextDouble() * 30 - 5), 2);
                    float humidity = (float)Math.Round((double)(random.NextDouble() * 70 + 20), 2);
                    float lowTemp = (float)Math.Round((double)(temp - (float)(random.NextDouble() * 3)), 2);
                    float highTemp = (float)Math.Round((double)(temp + (float)(random.NextDouble() * 5)), 2);

                    if (random.NextDouble() < 0.10)
                    {
                        temp = 42f;
                        humidity = 18f;
                    }
                    if (random.NextDouble() < 0.10)
                    {
                        temp = 25f;
                        humidity = 78f;
                    }
                    if (random.NextDouble() < 0.05)
                    {
                        temp = -2f;
                    }
                    if (random.NextDouble() < 0.08)
                    {
                        humidity = 92f;
                    }

                    environmentData.Add(new EnvironmentData
                    {
                        Postcode = postcode,
                        Street = street,
                        BinNumber = binNumber,
                        Temperature = (float)Math.Round((double)temp, 2),
                        Humidity = (float)Math.Round((double)humidity, 2),
                        LowTemp = (float)Math.Round((double)lowTemp, 2),
                        HighTemp = (float)Math.Round((double)highTemp, 2),
                        Timestamp = timestamp
                    });
                }
            }
        }

        context.SensorReadings.AddRange(sensorData);
        context.EnvironmentReadings.AddRange(environmentData);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with analysis data for sensor readings.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method generates synthetic analysis data for bins over a 28-day period.
    /// Each bin is assigned a usage profile (light, moderate, heavy) that determines
    /// the rate of fill growth, reset behavior, and reset cycle. The last bin is
    /// configured to always remain nearly full for testing purposes.
    /// </remarks>
    public static async Task SeedAnalysisDataAsync(ApplicationDbContext context)
    {
        if (await context.SensorAnalysisReadings.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var random = new Random(1234);

        int daysBack = 28;
        int binCount = 5;

        var sensorAnalysisData = new List<SensorAnalysisData>();

        // Define usage profiles: light, moderate, heavy
        var binUsageProfiles = new[]
        {
            new { FillGrowth = (1.0f, 2.0f), ResetFill = (3f, 8f), ResetCycle = 20 }, // Light usage
            new { FillGrowth = (1.5f, 4.0f), ResetFill = (5f, 15f), ResetCycle = 14 }, // Moderate usage
            new { FillGrowth = (3.0f, 6.0f), ResetFill = (10f, 20f), ResetCycle = 10 }, // Heavy usage
        };

        for (int binId = 1; binId <= binCount; binId++)
        {
            float fill;
            float weight;

            bool isAlwaysFull = binId == 5; // Last bin always near full

            // Initialise values
            fill = isAlwaysFull
                ? (float)(random.NextDouble() * 10 + 85) // Start 85-95% full
                : (float)(random.NextDouble() * 10 + 20); // Start 20-30%

            weight = (float)(random.NextDouble() * 5 + 2); // Start 2-7kg

            var usageProfile = isAlwaysFull
                ? new { FillGrowth = (2.0f, 5.0f), ResetFill = (80f, 90f), ResetCycle = 100 } // Rarely resets, stays full
                : binUsageProfiles[(binId - 1) % binUsageProfiles.Length];

            for (int day = 0; day <= daysBack; day++)
            {
                var timestamp = now.AddDays(-daysBack + day);

                // Reset logic
                if (!isAlwaysFull && day != 0 && day % usageProfile.ResetCycle == 0)
                {
                    fill = (float)(random.NextDouble() * (usageProfile.ResetFill.Item2 - usageProfile.ResetFill.Item1) + usageProfile.ResetFill.Item1);
                    weight = (float)(random.NextDouble() * 5 + 2); // Reset to 2-7kg
                }
                else
                {
                    // Increase fill
                    float fillIncrease = (float)(random.NextDouble() * (usageProfile.FillGrowth.Item2 - usageProfile.FillGrowth.Item1) + usageProfile.FillGrowth.Item1);
                    fill = Math.Min(fill + fillIncrease, 100);

                    // Add some random sensor noise
                    fill += (float)(random.NextDouble() * 2 - 1);     // +/- 1%
                    weight += (float)(random.NextDouble() * 0.5 - 0.25); // +/- 0.25kg

                    // Prevent overflow
                    fill = Math.Min(fill, 100);
                    weight = Math.Min(weight, 25);
                }

                sensorAnalysisData.Add(new SensorAnalysisData
                {
                    Postcode = "TS18",
                    Street = $"Test Street {binId}",
                    BinNumber = binId,
                    FillLevel = (float)Math.Round(fill, 2),
                    Weight = (float)Math.Round(weight, 2),
                    Timestamp = timestamp
                });
            }
        }
        context.SensorAnalysisReadings.AddRange(sensorAnalysisData);
        await context.SaveChangesAsync();
    }
}
