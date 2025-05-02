namespace Bindicator.Helpers
{
    /// <summary>
    /// Provides helper methods for working with <see cref="DateTime"/> objects.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Converts a given UTC timestamp to a human-readable relative time string.
        /// </summary>
        /// <param name="timestampUtc">The UTC timestamp to convert.</param>
        /// <returns>
        /// A string representing the relative time difference between the given timestamp
        /// and the current UTC time. For example, "2 days, 3 hours & 15 minutes ago".
        /// </returns>
        public static string ToNaturalTime(this DateTime timestampUtc)
        {
            var diff = DateTime.UtcNow - timestampUtc;

            int days = (int)diff.TotalDays;
            int hours = diff.Hours;
            int minutes = diff.Minutes;

            var parts = new List<string>();

            if (days > 0)
                parts.Add($"{days} day{(days > 1 ? "s" : "")}");
            if (hours > 0)
                parts.Add($"{hours} hour{(hours > 1 ? "s" : "")}");
            if (minutes > 0 || parts.Count == 0) // Show minutes even if 0 for "just now"
                parts.Add($"{minutes} minute{(minutes != 1 ? "s" : "")}");

            return string.Join(", ", parts.Take(parts.Count - 1)) +
                   (parts.Count > 1 ? " & " : "") +
                   parts.Last() + " ago";
        }
    }
}