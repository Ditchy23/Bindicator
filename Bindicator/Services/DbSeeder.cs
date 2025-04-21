using Bindicator.Data;
using Bindicator.Models;
using Microsoft.EntityFrameworkCore;

public class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.SensorReadings.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var sensorData = new List<SensorData>
        {
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, FillLevel = 32.38f, Weight = 6.46f, Density = 1.51f, Timestamp = now.AddDays(-5), Latitude = 54.5631, Longitude = -1.3123 },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, FillLevel = 69.31f, Weight = 11.54f, Density = 1.22f, Timestamp = now.AddDays(-4), Latitude = 54.5631, Longitude = -1.3123 },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, FillLevel = 79.09f, Weight = 9.73f, Density = 0.8f, Timestamp = now.AddDays(-3), Latitude = 54.5631, Longitude = -1.3123 },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, FillLevel = 40.13f, Weight = 5.62f, Density = 1.5f, Timestamp = now.AddDays(-2), Latitude = 54.5631, Longitude = -1.3123 },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, FillLevel = 72.5f, Weight = 17.25f, Density = 1.46f, Timestamp = now.AddDays(-1), Latitude = 54.5631, Longitude = -1.3123 },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, FillLevel = 23.59f, Weight = 18.68f, Density = 1.36f, Timestamp = now.AddDays(-5), Latitude = 54.5662, Longitude = -1.315 },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, FillLevel = 76.44f, Weight = 18.29f, Density = 0.65f, Timestamp = now.AddDays(-4), Latitude = 54.5662, Longitude = -1.315 },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, FillLevel = 75.05f, Weight = 12.84f, Density = 0.93f, Timestamp = now.AddDays(-3), Latitude = 54.5662, Longitude = -1.315 },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, FillLevel = 60.4f, Weight = 10.13f, Density = 0.71f, Timestamp = now.AddDays(-2), Latitude = 54.5662, Longitude = -1.315 },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, FillLevel = 18.14f, Weight = 6.92f, Density = 1.11f, Timestamp = now.AddDays(-1), Latitude = 54.5662, Longitude = -1.315 },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, FillLevel = 83.78f, Weight = 7.73f, Density = 1.07f, Timestamp = now.AddDays(-5), Latitude = 54.5688, Longitude = -1.311 },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, FillLevel = 57.87f, Weight = 5.11f, Density = 1.55f, Timestamp = now.AddDays(-4), Latitude = 54.5688, Longitude = -1.311 },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, FillLevel = 64.9f, Weight = 15.17f, Density = 1.59f, Timestamp = now.AddDays(-3), Latitude = 54.5688, Longitude = -1.311 },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, FillLevel = 79.33f, Weight = 6.88f, Density = 0.96f, Timestamp = now.AddDays(-2), Latitude = 54.5688, Longitude = -1.311 },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, FillLevel = 69.88f, Weight = 9.29f, Density = 0.87f, Timestamp = now.AddDays(-1), Latitude = 54.5688, Longitude = -1.311 },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, FillLevel = 57.35f, Weight = 15.82f, Density = 1.23f, Timestamp = now.AddDays(-5), Latitude = 54.5645, Longitude = -1.3105 },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, FillLevel = 77.47f, Weight = 17.86f, Density = 0.95f, Timestamp = now.AddDays(-4), Latitude = 54.5645, Longitude = -1.3105 },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, FillLevel = 16.49f, Weight = 5.83f, Density = 0.76f, Timestamp = now.AddDays(-3), Latitude = 54.5645, Longitude = -1.3105 },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, FillLevel = 33.68f, Weight = 15.63f, Density = 1.22f, Timestamp = now.AddDays(-2), Latitude = 54.5645, Longitude = -1.3105 },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, FillLevel = 58.54f, Weight = 15.06f, Density = 1.01f, Timestamp = now.AddDays(-1), Latitude = 54.5645, Longitude = -1.3105 },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, FillLevel = 21.14f, Weight = 16.5f, Density = 0.93f, Timestamp = now.AddDays(-5), Latitude = 54.5671, Longitude = -1.3161 },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, FillLevel = 56.87f, Weight = 8.84f, Density = 0.85f, Timestamp = now.AddDays(-4), Latitude = 54.5671, Longitude = -1.3161 },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, FillLevel = 11.2f, Weight = 18.28f, Density = 0.99f, Timestamp = now.AddDays(-3), Latitude = 54.5671, Longitude = -1.3161 },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, FillLevel = 40.13f, Weight = 11.22f, Density = 0.96f, Timestamp = now.AddDays(-2), Latitude = 54.5671, Longitude = -1.3161 },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, FillLevel = 50.45f, Weight = 19.99f, Density = 0.81f, Timestamp = now.AddDays(-1), Latitude = 54.5671, Longitude = -1.3161 }
        };

        var environmentData = new List<EnvironmentData>
        {
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, Temperature = 17.88f, Humidity = 70.95f, LowTemp = 8.24f, HighTemp = 28.56f, Timestamp = now.AddDays(-5) },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, Temperature = 17.12f, Humidity = 73.64f, LowTemp = 6.36f, HighTemp = 25.79f, Timestamp = now.AddDays(-4) },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, Temperature = 17.08f, Humidity = 56.14f, LowTemp = 7.99f, HighTemp = 28.97f, Timestamp = now.AddDays(-3) },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, Temperature = 20.63f, Humidity = 60.54f, LowTemp = 9.72f, HighTemp = 29.42f, Timestamp = now.AddDays(-2) },
            new() { Postcode = "TS16", Street = "Formby Walk", BinNumber = 1, Temperature = 20.61f, Humidity = 55.9f, LowTemp = 5.41f, HighTemp = 29.27f, Timestamp = now.AddDays(-1) },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, Temperature = 24.73f, Humidity = 64.69f, LowTemp = 9.33f, HighTemp = 24.27f, Timestamp = now.AddDays(-5) },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, Temperature = 18.48f, Humidity = 73.52f, LowTemp = 7.16f, HighTemp = 25.04f, Timestamp = now.AddDays(-4) },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, Temperature = 21.93f, Humidity = 79.05f, LowTemp = 6.92f, HighTemp = 28.31f, Timestamp = now.AddDays(-3) },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, Temperature = 21.47f, Humidity = 60.59f, LowTemp = 6.28f, HighTemp = 26.15f, Timestamp = now.AddDays(-2) },
            new() { Postcode = "TS17", Street = "Oakwood Drive", BinNumber = 2, Temperature = 22.97f, Humidity = 74.36f, LowTemp = 5.32f, HighTemp = 23.76f, Timestamp = now.AddDays(-1) },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, Temperature = 19.5f, Humidity = 52.64f, LowTemp = 7.8f, HighTemp = 24.11f, Timestamp = now.AddDays(-5) },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, Temperature = 20.15f, Humidity = 70.04f, LowTemp = 9.4f, HighTemp = 26.81f, Timestamp = now.AddDays(-4) },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, Temperature = 24.6f, Humidity = 50.66f, LowTemp = 8.43f, HighTemp = 24.28f, Timestamp = now.AddDays(-3) },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, Temperature = 15.08f, Humidity = 52.97f, LowTemp = 6.6f, HighTemp = 22.93f, Timestamp = now.AddDays(-2) },
            new() { Postcode = "TS18", Street = "Cedar Avenue", BinNumber = 3, Temperature = 22.5f, Humidity = 73.52f, LowTemp = 5.1f, HighTemp = 25.81f, Timestamp = now.AddDays(-1) },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, Temperature = 23.14f, Humidity = 72.8f, LowTemp = 7.06f, HighTemp = 28.69f, Timestamp = now.AddDays(-5) },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, Temperature = 19.33f, Humidity = 53.24f, LowTemp = 7.83f, HighTemp = 28.21f, Timestamp = now.AddDays(-4) },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, Temperature = 21.19f, Humidity = 79.33f, LowTemp = 5.22f, HighTemp = 24.62f, Timestamp = now.AddDays(-3) },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, Temperature = 22.66f, Humidity = 59.61f, LowTemp = 7.82f, HighTemp = 27.69f, Timestamp = now.AddDays(-2) },
            new() { Postcode = "TS16", Street = "Alder Crescent", BinNumber = 4, Temperature = 24.19f, Humidity = 51.96f, LowTemp = 7.96f, HighTemp = 25.86f, Timestamp = now.AddDays(-1) },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, Temperature = 24.47f, Humidity = 79.63f, LowTemp = 6.73f, HighTemp = 28.36f, Timestamp = now.AddDays(-5) },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, Temperature = 16.76f, Humidity = 54.19f, LowTemp = 9.26f, HighTemp = 26.83f, Timestamp = now.AddDays(-4) },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, Temperature = 23.08f, Humidity = 74.12f, LowTemp = 8.13f, HighTemp = 28.11f, Timestamp = now.AddDays(-3) },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, Temperature = 23.89f, Humidity = 65.48f, LowTemp = 5.39f, HighTemp = 25.82f, Timestamp = now.AddDays(-2) },
            new() { Postcode = "TS17", Street = "Birch Lane", BinNumber = 5, Temperature = 18.39f, Humidity = 56.02f, LowTemp = 5.24f, HighTemp = 29.05f, Timestamp = now.AddDays(-1) }
        };

        context.SensorReadings.AddRange(sensorData);
        context.EnvironmentReadings.AddRange(environmentData);
        await context.SaveChangesAsync();
    }

    private static float GetRandomFloat(float min, float max)
    {
        var rand = new Random();
        return (float)(rand.NextDouble() * (max - min) + min);
    }
}
