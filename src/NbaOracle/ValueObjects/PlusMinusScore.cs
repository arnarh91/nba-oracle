using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class PlusMinusScore : ValueObject
    {
        public decimal Score { get; }

        public PlusMinusScore(string value)
        {
            _ = value.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(value));
            
            Score = ParsingMethods.ToDecimal(value.Trim());
        }

        public static implicit operator decimal(PlusMinusScore score) => score.Score;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Score;
        }
    }
}