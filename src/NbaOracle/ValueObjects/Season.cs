using System.Collections.Generic;

namespace NbaOracle.ValueObjects
{
    public class Season : ValueObject
    {
        public int SeasonStartYear { get; }
        public int SeasonEndYear { get; }

        public Season(int seasonStartYear)
        {
            SeasonStartYear = seasonStartYear;
            SeasonEndYear = seasonStartYear + 1;
        }

        public IReadOnlyList<Month> GetMonthsInSeason()
        {
            return SeasonStartYear switch
            {
                2020 => new List<Month>
                {
                    Month.December(SeasonStartYear),
                    Month.January(SeasonEndYear),
                    Month.February(SeasonEndYear),
                    Month.March(SeasonEndYear),
                    Month.April(SeasonEndYear),
                    Month.May(SeasonEndYear),
                    Month.June(SeasonEndYear),
                    Month.July(SeasonEndYear)
                },
                2019 => new List<Month>
                {
                    Month.October(SeasonStartYear),
                    Month.November(SeasonStartYear),
                    Month.December(SeasonStartYear),
                    Month.January(SeasonEndYear),
                    Month.February(SeasonEndYear),
                    Month.March(SeasonEndYear),
                    Month.July(SeasonEndYear),
                    Month.August(SeasonEndYear),
                    Month.September(SeasonEndYear),
                    Month.October(SeasonEndYear)
                },
                2011 => new List<Month>
                {
                    Month.December(SeasonStartYear),
                    Month.January(SeasonEndYear),
                    Month.February(SeasonEndYear),
                    Month.March(SeasonEndYear),
                    Month.April(SeasonEndYear),
                    Month.May(SeasonEndYear),
                    Month.June(SeasonEndYear)
                },
                _ => new List<Month>
                {
                    Month.October(SeasonStartYear),
                    Month.November(SeasonStartYear),
                    Month.December(SeasonStartYear),
                    Month.January(SeasonEndYear),
                    Month.February(SeasonEndYear),
                    Month.March(SeasonEndYear),
                    Month.April(SeasonEndYear),
                    Month.May(SeasonEndYear),
                    Month.June(SeasonEndYear)
                }
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SeasonEndYear;
        }
    }
}