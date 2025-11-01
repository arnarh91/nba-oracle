using System;

namespace NbaOracle.Extensions;

public static class ObjectValidationExtensions
{
    public static object DiscardNullOrWhitespaceCheck(this string value)
    {
        return !string.IsNullOrWhiteSpace(value) ? value : null;
    }
}

public static class StringExtensions
{
    public static string NormalizeName(this string self)
    {
        return string.Join(" ", self.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));
    }
}