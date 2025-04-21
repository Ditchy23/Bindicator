namespace Bindicator.ViewModels
{
    /// <summary>
    /// ViewModel representing a bin location for route planning.
    /// </summary>
    public class RouteBinPointViewModel
    {
        /// <summary>
        /// Gets or sets the postcode of the bin.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street name where the bin is located.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the bin.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the bin.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bin is predicted to be full.
        /// </summary>
        public bool IsPredictedFull { get; set; }
    }
}
