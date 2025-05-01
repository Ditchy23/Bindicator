namespace Bindicator.Models
{
    /// <summary>
    /// Represents the analysis data for a sensor, including location, bin details, and measurements.
    /// </summary>
    public class SensorAnalysisData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sensor analysis data.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the postcode of the location where the sensor is installed.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street name of the location where the sensor is installed.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number associated with the sensor.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the fill level of the bin as measured by the sensor.
        /// </summary>
        public float FillLevel { get; set; }

        /// <summary>
        /// Gets or sets the weight of the contents in the bin as measured by the sensor.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the data was recorded.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
