namespace Bindicator.ViewModels
{
    /// <summary>
    /// ViewModel for editing the location of a bin.
    /// </summary>
    public class EditLocationViewModel
    {
        /// <summary>
        /// Gets or sets the postcode of the bin's location.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street of the bin's location.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number.
        /// </summary>
        public int BinNumber { get; set; }

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