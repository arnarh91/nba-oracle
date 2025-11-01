using System;
using System.Globalization;

namespace NbaOracle.Extensions
{
    public static class ParsingMethods
    {
        public static int ToInt(string value)
        {
            if (int.TryParse(value, out var result))
                return result;

            throw new FormatException($"Unable to format string to int. ({value})");
        }

        public static decimal ToDecimal(string value)
        {
            if (decimal.TryParse(value, out var result))
                return result;

            throw new FormatException($"Unable to format string to double. ({value})");
        }

        public static DateTime ToDate(string value, string format)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            throw new FormatException($"Unable to format string to int. ({value})");
        }
    }
}