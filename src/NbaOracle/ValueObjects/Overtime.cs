using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class Overtime : ValueObject
    {
        public int Count { get; }

        public Overtime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Count = 0;
                return;
            }

            value = value.Trim().ToLower(CultureInfo.InvariantCulture);

            if (value.Length == 2 && value == "ot")
            {
                Count = 1;
                return;
            }

            value = value.Replace("ot", "");

            if (!value.Any(char.IsDigit))
                throw new ArgumentException("Overtime value is formatted incorrectly");

            Count = ParsingMethods.ToInt(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Count;
        }
    }
}