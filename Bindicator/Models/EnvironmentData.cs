namespace Bindicator.Models
{
    /// <summary>
    /// Represents environmental data associated with a specific location and time.
    /// </summary>
    public class EnvironmentData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the environment data.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the postcode of the location.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street name of the location.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number associated with the location.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the temperature recorded at the location.
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// Gets or sets the humidity level recorded at the location.
        /// </summary>
        public float Humidity { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the data was recorded.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
