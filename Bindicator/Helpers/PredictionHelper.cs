using Bindicator.Models;

namespace Bindicator.Helpers
{
    public static class PredictionHelper
    {
        /// <summary>
        /// Calculates the predicted date when the bin will be full based on the recent trend of fill levels.
        /// </summary>
        /// <param name="readings">The list of sensor readings.</param>
        /// <param name="model">The view model containing bin details.</param>
        public static void CalculatePredictedFullDate(List<SensorData> readings, dynamic model)
        {
            if (readings.Count >= 3)
            {
                // Use last 3 readings for a recent trend
                var recent = readings.Skip(Math.Max(0, readings.Count - 3)).ToList();

                // Ensure all readings are increasing
                if (recent[0].FillLevel < recent[1].FillLevel && recent[1].FillLevel < recent[2].FillLevel)
                {
                    var first = recent[0];
                    var last = recent[2];
                    double deltaFill = last.FillLevel - first.FillLevel;
                    double days = (last.Timestamp - first.Timestamp).TotalDays;

                    if (deltaFill > 0 && days > 0)
                    {
                        double ratePerDay = deltaFill / days;
                        double remaining = 100.0 - last.FillLevel;
                        double daysToFull = remaining / ratePerDay;
                        // Clamp to max 21 days for demo
                        daysToFull = Math.Min(daysToFull, 21);

                        model.PredictedFullDate = last.Timestamp.AddDays(daysToFull);
                        model.DaysToFull = daysToFull;
                        return;
                    }
                }
            }
            model.PredictedFullDate = null;
            model.DaysToFull = null;
        }

        // Add more helper methods if needed for other prediction or calculations.
    }
}
