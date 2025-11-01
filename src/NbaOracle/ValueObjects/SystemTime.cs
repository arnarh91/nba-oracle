using System;

namespace NbaOracle.ValueObjects
{
    public static class SystemTime
    {
        public static Func<DateTime> Now { get; set; } = () => DateTime.Now;

        public static void Set(DateTime date)
        {
            Now = () => date;
        }

        public static void Reset()
        {
            Now = () => DateTime.Now;
        }
    }
}