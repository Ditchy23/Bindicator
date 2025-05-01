using System.Text.Json.Serialization;

namespace Bindicator.ViewModels
{
    public class BinAnalysisResult
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonPropertyName("current_weight")]
        public float CurrentWeight { get; set; }

        [JsonPropertyName("current_fill")]
        public float CurrentFill { get; set; }

        [JsonPropertyName("weight_rate_per_day")]
        public float WeightRatePerDay { get; set; }

        [JsonPropertyName("fill_rate_per_day")]
        public float FillRatePerDay { get; set; }

        [JsonPropertyName("days_to_full_weight")]
        public float? DaysToFullWeight { get; set; }

        [JsonPropertyName("days_to_full_fill")]
        public float? DaysToFullFill { get; set; }

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } = string.Empty;
    }
}