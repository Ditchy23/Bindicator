namespace Bindicator.ViewModels
{
    public class SensorDataViewModel
    {
        /// <summary>
        /// Unique identifier for the bin.
        /// </summary>
        public int BinNumber { get; set; }
        /// <summary>
        /// Latitude of the bin's location.
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude of the bin's location.
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Current fill level of the bin as a percentage.
        /// </summary>
        public float FillLevel { get; set; }
        /// <summary>
        /// Current weight of the bin in kilograms.
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// Timestamp of the last reading.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Postcode of the bin's location.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;
        /// <summary>
        /// Street name of the bin's location.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        // Enhanced properties for the map or routing logic
        public double? DaysToFull { get; set; }
        /// <summary>
        /// Predicted date when the bin will be full based on recent trends.
        /// </summary>
        public DateTime? PredictedFullDate { get; set; }
        /// <summary>
        /// Indicates if the bin is full based on the fill level.
        /// </summary>
        public bool IsFull => FillLevel >= 85; // Example logic
        /// <summary>
        /// Maximum weight the bin can hold before it is considered full.
        /// </summary>
        public double MaxWeight { get; set; } = 25;
        /// <summary>
        /// Weight of the wagon or vehicle used for collection.
        /// </summary>
        public double WagonWeight { get; set; } = 200; // Example weight
    }
}