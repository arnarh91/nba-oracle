using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class NumberOfYearInLeague : ValueObject
    {
        private int Value { get; }

        public NumberOfYearInLeague(string value)
        {
            _ = value.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(value));

            value = value.Trim();

            Value = value.ToUpper() == "R"
                ? 1
                : ParsingMethods.ToInt(value) + 1;
        }

        public static implicit operator int(NumberOfYearInLeague value) => value.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}