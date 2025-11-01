using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects;

public class BoxScoreLink : ValueObject
{
    public string BoxScoreId { get; }
    public string GameIdentifier { get; }
    public DateTime GameDate { get; }

    public BoxScoreLink(string value)
    {
        _ = value.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(value));

        value = value.Trim();

        value = value.Replace(".html", "");

        if (!value.StartsWith("/boxscores/"))
            throw new ArgumentException("BoxScoreLink is not formatted correctly. Missing '/boxscores/' at the start of the string");

        BoxScoreId = value[11..];
        GameIdentifier = BoxScoreId[..8] + BoxScoreId[9..];
        GameDate = ParsingMethods.ToDate(BoxScoreId[..8], "yyyyMMdd");
    }

    public string ToHtmlLink() => $"boxscores/{BoxScoreId}.html";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BoxScoreId;
    }
}