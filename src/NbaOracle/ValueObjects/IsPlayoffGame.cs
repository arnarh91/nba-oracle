using System;
using System.Collections.Generic;

namespace NbaOracle.ValueObjects
{
    public class IsPlayoffGame : ValueObject
    {
        public bool Value { get; }
        public DateOnly GameDate { get; }

        public IsPlayoffGame(DateOnly gameDate)
        {
            GameDate = gameDate;
            Value = IsPlayoffGameCheck(gameDate);
        }

        public static implicit operator bool(IsPlayoffGame game) => game.Value;

        private static bool IsPlayoffGameCheck(DateOnly gameDate)
        {
            var date = gameDate.ToDateTime(TimeOnly.MinValue);
            return gameDate.Year switch
            {
                2025 => date >= new DateTime(2025, 4, 19) && date <= new DateTime(2025, 6, 22),
                2024 => date >= new DateTime(2024, 4, 20) && date <= new DateTime(2024, 6, 17),
                2023 => date >= new DateTime(2023, 4, 15) && date <= new DateTime(2023, 6, 12),
                2022 => date >= new DateTime(2022, 4, 16) && date <= new DateTime(2022, 6, 16),
                2021 => date >= new DateTime(2021, 5, 22) && date <= new DateTime(2021, 7, 20),
                2020 => date >= new DateTime(2020, 8, 17) && date <= new DateTime(2020, 10, 11),
                2019 => date >= new DateTime(2019, 4, 13) && date <= new DateTime(2019, 6, 13),
                2018 => date >= new DateTime(2018, 4, 14) && date <= new DateTime(2018, 6, 8),
                2017 => date >= new DateTime(2017, 4, 15) && date <= new DateTime(2017, 6, 12),
                2016 => date >= new DateTime(2016, 4, 16) && date <= new DateTime(2016, 6, 19),
                2015 => date >= new DateTime(2015, 4, 18) && date <= new DateTime(2015, 6, 16),
                2014 => date >= new DateTime(2014, 4, 19) && date <= new DateTime(2014, 6, 15),
                2013 => date >= new DateTime(2013, 4, 20) && date <= new DateTime(2013, 6, 20),
                2012 => date >= new DateTime(2012, 4, 28) && date <= new DateTime(2012, 6, 21),
                2011 => date >= new DateTime(2011, 4, 16) && date <= new DateTime(2011, 6, 12),
                2010 => date >= new DateTime(2010, 4, 17) && date <= new DateTime(2010, 6, 17),
                2009 => date >= new DateTime(2009, 4, 18) && date <= new DateTime(2009, 6, 14),
                _ => throw new ArgumentException($"Playoff starting date is not known for year = {gameDate.Year}")
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}