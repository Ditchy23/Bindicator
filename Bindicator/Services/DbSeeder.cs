using Bindicator.Data;
using Bindicator.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

/// <summary>
/// Class to seed the database with initial data.
/// </summary>
public class DbSeeder
{
    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
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

        // Streets for each postcode (you can adjust these or add more)
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

                // Postcode group offset as before, but each bin in group can have a different day offset
                int groupOffset = postcode switch
                {
                    "TS16" => 0,
                    "TS17" => 2,
                    "TS18" => 4,
                    _ => 0
                };

                // Each bin can have its own offset within the group to stagger collection
                int binOffset = (binIdx % 4); // 0,1,2,3,0,1,2,3...

                float fill = random.Next(10, 30);
                float weight = random.Next(4, 10);

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
                        // Bins fill steadily
                        fill = Math.Min(fill + (float)(random.NextDouble() * 7.5 + 2.5), 100);
                        weight = Math.Min(weight + (float)(random.NextDouble() * 2 + 0.8), 25);
                    }

                    // Optionally, for 1-2 bins per group, leave "just emptied" at the most recent day
                    if (day == 0 && (binIdx % 10 == 0 || binIdx % 10 == 7))
                    {
                        fill = random.Next(5, 18);
                        weight = random.Next(2, 7);
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

                    // ...environment data as before...
                    // (No changes needed to env data)
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
}
