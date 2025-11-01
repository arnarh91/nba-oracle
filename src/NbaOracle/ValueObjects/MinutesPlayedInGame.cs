using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class MinutesPlayedInGame : ValueObject
    {
        public int Minutes { get; }
        public int Seconds { get; }

        public MinutesPlayedInGame(string value)
        {
            _ = value.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(value));

            value = value.Trim();

            var split = value.Split(":");

            if (split.Length != 2)
                throw new ArgumentException("MinutesPlayedInGame value is not formatted correctly");

            Minutes = ParsingMethods.ToInt(split[0]);
            Seconds = ParsingMethods.ToInt(split[1]);
        }

        public int TotalSecondsPlayed() => Minutes * 60 + Seconds;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Minutes;
            yield return Seconds;
        }
    }
}