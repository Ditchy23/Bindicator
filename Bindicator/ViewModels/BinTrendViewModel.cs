using Bindicator.Models;

namespace Bindicator.ViewModels
{
    /// <summary>
    /// ViewModel representing the trend data for a specific bin.
    /// </summary>
    public class BinTrendViewModel
    {
        /// <summary>
        /// Gets or sets the postcode of the bin location.
        /// </summary>
        public string Postcode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street of the bin location.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bin number.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of sensor data readings.
        /// </summary>
        public List<SensorData> Readings { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of spike points indicating significant changes in bin levels.
        /// </summary>
        public List<SpikePoint> Spikes { get; set; } = new();

        // Prediction
        public DateTime? PredictedFullDate { get; set; }
        public double? DaysToFull { get; set; }
    }

    /// <summary>
    /// Represents a point in time where a significant change in bin level occurred.
    /// </summary>
    public class SpikePoint
    {
        /// <summary>
        /// Gets or sets the timestamp of the spike.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the bin level before the spike.
        /// </summary>
        public float FromLevel { get; set; }

        /// <summary>
        /// Gets or sets the bin level after the spike.
        /// </summary>
        public float ToLevel { get; set; }
    }
}
