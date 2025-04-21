namespace Bindicator.Models
{
    public class BinPredictionResult
    {
        public string Postcode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public int BinNumber { get; set; }
        public float CurrentWeight { get; set; }
        public float DailyIncreaseRate { get; set; }
        public DateTime? ExpectedFullDate { get; set; }
    }
}