using System;

namespace NbaOracle.WebScrapers.OddsPortal;

public record OddsPortalGameRecord
{
    public OddsPortalGameRecord(DateOnly date, string homeTeamIdentifier, string awayTeamIdentifier, decimal? homeTeamOdds, decimal? awayTeamOdds)
    {
        Date = date;
        HomeTeamIdentifier = homeTeamIdentifier;
        AwayTeamIdentifier = awayTeamIdentifier;
        HomeTeamOdds = homeTeamOdds;
        AwayTeamOdds = awayTeamOdds;

        IsMissingOdds = homeTeamOdds is null || awayTeamOdds is null;
    
        GameIdentifier = $"{date:yyyyMMdd}{HomeTeamIdentifier}";
    }

    public DateOnly Date { get; set; }
    public string GameIdentifier { get; set; }

    public string HomeTeamIdentifier { get; set; } = null!;
    public string AwayTeamIdentifier { get; set; } = null!;
    
    public bool IsMissingOdds { get; set; }
    public decimal? HomeTeamOdds { get; set; }
    public decimal? AwayTeamOdds { get; set; }
}