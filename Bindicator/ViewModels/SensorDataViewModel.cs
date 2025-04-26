namespace Bindicator.ViewModels
{
    public class SensorDataViewModel
    {
        public int BinNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float FillLevel { get; set; }
        public double Weight { get; set; }
        public DateTime Timestamp { get; set; }
        public string Postcode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;

        // Enhanced properties for the map or routing logic
        public double? DaysToFull { get; set; }
        public DateTime? PredictedFullDate { get; set; }
        public bool IsFull => FillLevel >= 85; // Example logic
        public double MaxWeight { get; set; } = 25;
        public double WagonWeight { get; set; } = 1000;
    }
}
