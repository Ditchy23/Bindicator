using Bindicator.Data;
using Bindicator.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

/// <summary>
/// Class responsible for seeding the database with initial data.
/// </summary>
public class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.SensorReadings.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // Center coordinates for each postcode
        var postcodeCoords = new Dictionary<string, (double Lat, double Lon)>
           {
               { "TS16", (54.525079, -1.3649298) },
               { "TS17", (54.5313629, -1.2914754) },
               { "TS18", (54.5529822, -1.3193432) }
           };

        // Streets for each postcode
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

                // Postcode group offset, but each bin in group can have a different day offset
                int groupOffset = postcode switch
                {
                    "TS16" => 0,
                    "TS17" => 2,
                    "TS18" => 4,
                    _ => 0
                };

                // Each bin can have its own offset within the group to stagger collection
                int binOffset = (binIdx % 4);

                float fill = random.Next(10, 30);
                float weight = random.Next(4, 10);

                // Ensure we have at least 3 recent readings with an increase in weight for prediction
                for (int day = 27; day >= 0; day--)
                {
                    var timestamp = now.AddDays(-day);

                    int daysSinceFirst = 27 - day;

                    // "Collection" every 14 days + bin offset to stagger empties in group
                    bool isCollectionDay = ((daysSinceFirst - groupOffset - binOffset) % 14 == 0);

                    if (isCollectionDay && daysSinceFirst > 0)
                    {
                        // Emptied bins don't always go back to nearly zero—simulate variable empties!
                        fill = random.Next(5, 18);
                        weight = random.Next(2, 7);
                    }
                    else
                    {
                        // Ensure weight and fill increase steadily for the last 3 readings
                        if (daysSinceFirst >= 24) // Ensure the last 3 readings increase
                        {
                            fill = Math.Min(fill + (float)(random.NextDouble() * 7.5 + 2.5), 100);
                            weight = Math.Min(weight + (float)(random.NextDouble() * 2 + 0.8), 25);
                        }
                        else
                        {
                            fill = Math.Min(fill + (float)(random.NextDouble() * 3 + 1), 100); // Steady increase for earlier days
                            weight = Math.Min(weight + (float)(random.NextDouble() * 1.5 + 0.5), 25); // Steady increase for earlier days
                        }
                    }

                    float density = (float)Math.Round(random.NextDouble() * 1.2 + 0.7, 2);

                    if (random.NextDouble() < 0.07)
                        fill = random.Next(85, 100);

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

                    // Environment data
                    float temp = (float)(random.NextDouble() * 30 - 5);
                    float humidity = (float)(random.NextDouble() * 70 + 20);
                    float lowTemp = temp - (float)(random.NextDouble() * 3);
                    float highTemp = temp + (float)(random.NextDouble() * 5);

                    // Occasionally force "warning" values for demo
                    if (random.NextDouble() < 0.10)
                    {
                        temp = 42; humidity = 18;
                    }
                    if (random.NextDouble() < 0.10)
                    {
                        temp = 25; humidity = 78;
                    }
                    if (random.NextDouble() < 0.05)
                    {
                        temp = -2;
                    }
                    if (random.NextDouble() < 0.08)
                    {
                        humidity = 92;
                    }

                    environmentData.Add(new EnvironmentData
                    {
                        Postcode = postcode,
                        Street = street,
                        BinNumber = binNumber,
                        Temperature = temp,
                        Humidity = humidity,
                        LowTemp = lowTemp,
                        HighTemp = highTemp,
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

            // Initialize values
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