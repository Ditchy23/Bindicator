namespace Bindicator.ViewModels
{
    /// <summary>
    /// ViewModel representing the status of a bin.
    /// </summary>
    public class BinStatusViewModel
    {
        /// <summary>
        /// Gets or sets the postcode where the bin is located.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street where the bin is located.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the fill level of the bin.
        /// </summary>
        public float FillLevel { get; set; }

        /// <summary>
        /// Gets or sets the weight of the bin's contents.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Gets or sets the density of the bin's contents.
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the bin status.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the bin's location.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the bin's location.
        /// </summary>
        public double Longitude { get; set; }
    }
}