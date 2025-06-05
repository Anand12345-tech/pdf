using System;

namespace PdfManagement.Core.Domain.Extensions
{
    /// <summary>
    /// Extension methods for DateTime handling
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime to a format suitable for PostgreSQL timestamp without time zone
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>A DateTime with Kind = Unspecified</returns>
        public static DateTime ToUnspecifiedKind(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }
        
        /// <summary>
        /// Gets the current UTC time as a DateTime with Kind = Unspecified
        /// for use with PostgreSQL timestamp without time zone columns
        /// </summary>
        /// <returns>Current UTC time with Kind = Unspecified</returns>
        public static DateTime GetUtcNowAsUnspecified()
        {
            return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        }
    }
}
