using Bindicator.Models;
using Bindicator.ViewModels;

namespace Bindicator.Helpers
{
    public static class PredictionHelper
    {
        /// <summary>
        /// Calculates the predicted date when the bin will be full based on the recent trend of fill levels.
        /// </summary>
        /// <param name="readings">The list of sensor readings.</param>
        /// <param name="model">
        /// The view model containing bin details. This can be either:
        /// <see cref="SensorDataViewModel"/> or <see cref="BinTrendViewModel"/>.
        /// </param>
        public static void CalculatePredictedFullDate(List<SensorData> readings, dynamic model)
        {
            Console.WriteLine($"Calculating predicted full date for Bin #{model.BinNumber}");

            if (readings.Count >= 3)
            {
                // Use the last 3 readings for a recent trend
                var recent = readings.OrderBy(r => r.Timestamp).TakeLast(3).ToList();

                // Ensure all readings are increasing
                if (recent[0].FillLevel <= recent[1].FillLevel && recent[1].FillLevel <= recent[2].FillLevel)
                {
                    Console.WriteLine("Found increasing fill levels, calculating prediction.");

                    var first = recent[0];
                    var last = recent[2];
                    double deltaFill = last.FillLevel - first.FillLevel;
                    double days = (last.Timestamp - first.Timestamp).TotalDays;

                    if (deltaFill > 0 && days > 0)
                    {
                        double ratePerDay = deltaFill / days;
                        double remaining = 100.0 - last.FillLevel;
                        double daysToFull = remaining / ratePerDay;

                        // Clamp the days to full to a max of 21 days for demo purposes
                        daysToFull = Math.Min(daysToFull, 21);

                        // Check the type of model to set the correct properties
                        if (model is SensorDataViewModel sensorDataModel)
                        {
                            sensorDataModel.PredictedFullDate = last.Timestamp.AddDays(daysToFull);
                            sensorDataModel.DaysToFull = daysToFull;
                        }
                        else if (model is BinTrendViewModel binTrendModel)
                        {
                            binTrendModel.PredictedFullDate = last.Timestamp.AddDays(daysToFull);
                            binTrendModel.DaysToFull = daysToFull;
                        }

                        Console.WriteLine($"Prediction for Bin #{model.BinNumber}: {model.PredictedFullDate}, DaysToFull: {model.DaysToFull}");
                        return;
                    }
                }
            }

            // If there's no prediction, set both properties to null
            if (model is SensorDataViewModel sensorDataModelNull)
            {
                sensorDataModelNull.PredictedFullDate = null;
                sensorDataModelNull.DaysToFull = null;
            }
            else if (model is BinTrendViewModel binTrendModelNull)
            {
                binTrendModelNull.PredictedFullDate = null;
                binTrendModelNull.DaysToFull = null;
            }
            Console.WriteLine($"No prediction for Bin #{model.BinNumber}. Setting to null.");
        }
    }
}