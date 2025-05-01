using System.Text.Json.Serialization;

namespace Bindicator.ViewModels
{
    /// <summary>
    /// Represents the analysis result of a bin, including its current state and predictions.
    /// </summary>
    public class BinAnalysisResult
    {
        /// <summary>
        /// Gets or sets the unique identifier of the bin.
        /// </summary>
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        /// <summary>
        /// Gets or sets the current weight of the bin in kilograms.
        /// </summary>
        [JsonPropertyName("current_weight")]
        public float CurrentWeight { get; set; }

        /// <summary>
        /// Gets or sets the current fill level of the bin as a percentage.
        /// </summary>
        [JsonPropertyName("current_fill")]
        public float CurrentFill { get; set; }

        /// <summary>
        /// Gets or sets the rate of weight increase per day in kilograms.
        /// </summary>
        [JsonPropertyName("weight_rate_per_day")]
        public float WeightRatePerDay { get; set; }

        /// <summary>
        /// Gets or sets the rate of fill level increase per day as a percentage.
        /// </summary>
        [JsonPropertyName("fill_rate_per_day")]
        public float FillRatePerDay { get; set; }

        /// <summary>
        /// Gets or sets the estimated number of days until the bin is full by weight.
        /// </summary>
        [JsonPropertyName("days_to_full_weight")]
        public float? DaysToFullWeight { get; set; }

        /// <summary>
        /// Gets or sets the estimated number of days until the bin is full by fill level.
        /// </summary>
        [JsonPropertyName("days_to_full_fill")]
        public float? DaysToFullFill { get; set; }

        /// <summary>
        /// Gets or sets the recommendation for managing the bin.
        /// </summary>
        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } = string.Empty;
    }
}