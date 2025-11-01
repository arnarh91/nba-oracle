using System.Collections.Generic;

namespace NbaOracle.ValueObjects
{
    public class College : ValueObject
    {
        public string Value { get; }

        public College(string value)
        {
            Value = string.IsNullOrWhiteSpace(value) ? "High School" : value;
        }

        public static implicit operator string(College college) => college.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}