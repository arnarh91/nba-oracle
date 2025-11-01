using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class WeightInPounds : ValueObject
    {
        public int Value { get; }

        public WeightInPounds(string value)
        {
            _ = value.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(value));

            Value = ParsingMethods.ToInt(value);
        }

        public decimal ToKiloGrams()
        {
            return Math.Round(Value * 0.453592m, 2);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}