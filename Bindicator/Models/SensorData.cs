namespace Bindicator.Models
{
    /// <summary>
    /// Represents the data collected from a sensor.
    /// </summary>
    public class SensorData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sensor data.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the postcode where the sensor is located.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street where the sensor is located.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number associated with the sensor.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the fill level of the bin.
        /// </summary>
        public float FillLevel { get; set; }

        /// <summary>
        /// Gets or sets the weight of the contents in the bin.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Gets or sets the density of the contents in the bin.
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the data was collected.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the latitude where the sensor is located.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude where the sensor is located.
        /// </summary>
        public double Longitude { get; set; }
    }
}