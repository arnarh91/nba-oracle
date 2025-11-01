using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace NbaOracle.Data.GameBettingOdds;

public record GameBettingOddsData
{
    public GameBettingOddsData(string bettingProviderName, string gameIdentifier, decimal homeOdds, decimal awayOdds, DateTime scrapedAt)
    {
        BettingProviderName = bettingProviderName;
        GameIdentifier = gameIdentifier;
        HomeOdds = homeOdds;
        AwayOdds = awayOdds;
        HomeConfidence = Math.Round(1.0m / homeOdds, 5);
        AwayConfidence = Math.Round(1.0m / awayOdds, 5);
        ScrapedAt = scrapedAt;
    }

    public string BettingProviderName { get; init; }
    public string GameIdentifier { get; init; }
    public decimal HomeOdds { get; init; }
    public decimal AwayOdds { get; init; }
    public decimal HomeConfidence { get; init; }
    public decimal AwayConfidence { get; init; }
    public DateTime ScrapedAt { get; init; }
}

public class GameBettingOddsRepository
{
    private readonly IDbConnection _dbConnection;

    public GameBettingOddsRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task Merge(List<GameBettingOddsData> gameBettingOdds)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("BettingProviderName", typeof(string));
        dt.Columns.Add("GameIdentifier", typeof(string));
        dt.Columns.Add("HomeOdds", typeof(decimal));
        dt.Columns.Add("AwayOdds", typeof(decimal));
        dt.Columns.Add("HomeConfidence", typeof(decimal));
        dt.Columns.Add("AwayConfidence", typeof(decimal));
        dt.Columns.Add("ScrapedAt", typeof(DateTime));

        foreach (var gameOdds in gameBettingOdds)
        {
            dt.Rows.Add(gameOdds.BettingProviderName, gameOdds.GameIdentifier, gameOdds.HomeOdds, gameOdds.AwayOdds, gameOdds.HomeConfidence, gameOdds.AwayConfidence, gameOdds.ScrapedAt);
        }
            
        var parameters = new DynamicParameters();
        parameters.Add("@Odds", dt.AsTableValuedParameter("nba.tt_Merge_GameBettingOdds"));
        
        await _dbConnection.ExecuteAsync("nba.sp_MergeGameBettingOdds", parameters, commandType:CommandType.StoredProcedure);
    }
}