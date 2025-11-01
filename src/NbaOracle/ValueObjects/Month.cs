using System;
using System.Collections.Generic;
using System.Linq;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects
{
    public class Month : ValueObject
    {
        public string Name { get; }
        public int Year { get; }

        private static readonly IReadOnlyList<string> Months =
            new[]
            {
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December"
            };

        public Month(string name, int year)
        {
            _ = name.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(name));

            name = name.Trim();

            if (!Months.Contains(name))
                throw new ArgumentOutOfRangeException(nameof(name), $"Name '{name}' is not a valid month");

            Name = name;
            Year = year;
        }

        public string ToLower()
        {
            return Name.ToLower();
        }

        public bool IsOctoberDuringEither2019Or2020()
        {
            return Name == "October" && (Year == 2019 || Year == 2020);
        }
        
        public static Month January(int year) => new Month("January", year);
        public static Month February(int year) => new Month("February", year);
        public static Month March(int year) => new Month("March", year);
        public static Month April(int year) => new Month("April", year);
        public static Month May(int year) => new Month("May", year);
        public static Month June(int year) => new Month("June", year);
        public static Month July(int year) => new Month("July", year);
        public static Month August(int year) => new Month("August", year);
        public static Month September(int year) => new Month("September", year);
        public static Month October(int year) => new Month("October", year);
        public static Month November(int year) => new Month("November", year);
        public static Month December(int year) => new Month("December", year);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}