using System;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.Games;

public record GameData
{
    public GameData(DateOnly gameDate, string awayTeam, string homeTeam, int awayPoints, int homePoints, string boxScoreLink, string overtimes, int attendance)
    {
        GameDate = gameDate;
        
        AwayTeam = awayTeam;
        HomeTeam = homeTeam;
        WinningTeam = homePoints > awayPoints ? homeTeam : awayTeam;
        AwayPoints = awayPoints;
        HomePoints = homePoints;
        BoxScoreLink = boxScoreLink;
        NumberOfOvertimes = new Overtime(overtimes).Count;
        Attendance = attendance;
        IsPlayoffGame = new IsPlayoffGame(gameDate);
    }

    public DateOnly GameDate { get; }
        
    public string AwayTeam { get; }
    public string HomeTeam { get; }
    public string WinningTeam { get; }
        
    public int AwayPoints { get; }
    public int HomePoints { get; }

    public string BoxScoreLink { get; }

    public int NumberOfOvertimes { get; }
    public int Attendance { get; }

    public bool IsPlayoffGame { get; }
}