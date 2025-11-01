using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;

namespace NbaOracle.Data.Games;

public record GameModel
{
    public GameModel(DateOnly gameDate, string awayTeam, string homeTeam, int awayPoints, int homePoints, string boxScoreLink, int numberOfOvertimes, int attendance, bool isPlayoffGame)
    {
        GameDate = gameDate;
        GameIdentifier = $"{gameDate.ToString("yyyyMMdd")}{homeTeam}";
        AwayTeam = awayTeam;
        HomeTeam = homeTeam;
        WinningTeam = homePoints > awayPoints ? homeTeam : awayTeam;
        AwayPoints = awayPoints;
        HomePoints = homePoints;
        BoxScoreLink = boxScoreLink;
        NumberOfOvertimes = numberOfOvertimes;
        Attendance = attendance;
        IsPlayoffGame = isPlayoffGame;
    }

    public DateOnly GameDate { get; }
    public string GameIdentifier { get; }
        
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

public class GameRepository
{
    private readonly IDbConnection _dbConnection;

    public GameRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task Merge(Season season, List<GameModel> games)
    {
        var dt = new DataTable();
        dt.Columns.Add("GameIdentifier", typeof(string));
        dt.Columns.Add("SeasonId", typeof(int));
        dt.Columns.Add("GameDate", typeof(DateTime));
        dt.Columns.Add("HomeTeamIdentifier", typeof(string));
        dt.Columns.Add("AwayTeamIdentifier", typeof(string));
        dt.Columns.Add("WinTeamIdentifier", typeof(string));
        dt.Columns.Add("HomePoints", typeof(int));
        dt.Columns.Add("AwayPoints", typeof(int));
        dt.Columns.Add("BoxScoreLink", typeof(string));
        dt.Columns.Add("NumberOfOvertimes", typeof(int));
        dt.Columns.Add("Attendance", typeof(int));
        dt.Columns.Add("IsPlayoffGame", typeof(bool));

        foreach (var g in games)
        {
            dt.Rows.Add(g.GameIdentifier, season.SeasonStartYear, g.GameDate.ToDateTime(TimeOnly.MinValue), g.HomeTeam, g.AwayTeam, g.WinningTeam, g.HomePoints, g.AwayPoints, g.BoxScoreLink, g.NumberOfOvertimes, g.Attendance, g.IsPlayoffGame);
        }
            
        var parameters = new DynamicParameters();
        parameters.Add("@Games", dt.AsTableValuedParameter("nba.tt_Merge_Game"));
        
        await _dbConnection.ExecuteAsync("nba.sp_MergeGames", parameters, commandType:CommandType.StoredProcedure);
    }
}