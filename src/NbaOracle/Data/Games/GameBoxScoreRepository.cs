using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;

namespace NbaOracle.Data.Games;

public record GameBoxScore(int GameId, Season Season, List<GameQuarterScore> Quarters, List<GameDidNotPlay> DidNotPlay, List<PlayerBasicBoxScore> PlayerBasicBoxScore, List<PlayerAdvancedBoxScore> PlayerAdvancedBoxScore);
public record GameQuarterScore(int QuarterNumber, string QuarterLabel, int HomeScore, int AwayScore);
public record GameDidNotPlay(string TeamIdentifier, string PlayerName, string Reason);

public record PlayerBasicBoxScore(
    string PlayerName,
    string TeamIdentifier,
    bool Starter,
    int SecondsPlayed,
    int FieldGoalsMade,
    int FieldGoalsAttempted,
    int ThreePointersMade,
    int ThreePointersAttempted,
    int FreeThrowsMade,
    int FreeThrowsAttempted,
    int OffensiveRebounds,
    int DefensiveRebounds,
    int Assists,
    int Steals,
    int Blocks,
    int Turnovers,
    int PersonalFouls,
    int Points,
    decimal GameScore,
    int PlusMinusScore
)
{
    public int TotalRebounds => OffensiveRebounds + DefensiveRebounds;
}

public record PlayerAdvancedBoxScore(
    string PlayerName,
    string TeamIdentifier,
    decimal TrueShootingPercentage,
    decimal EffectiveFieldGoalPercentage,
    decimal ThreePointAttemptRate,
    decimal FreeThrowsAttemptRate,
    decimal OffensiveReboundPercentage,
    decimal DefensiveReboundPercentage,
    decimal TotalReboundPercentage,
    decimal AssistPercentage,
    decimal StealPercentage,
    decimal BlockPercentage,
    decimal TurnoverPercentage,
    decimal UsagePercentage,
    int OffensiveRating,
    int DefensiveRating,
    decimal BoxPlusMinus
);

public class GameBoxScoreRepository
{
    private readonly IDbConnection _dbConnection;

    public GameBoxScoreRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task Merge(List<GameBoxScore> boxScores)
    {
        var gameBoxScoreDataTable = CreateGameBoxScoreDataTable(boxScores);
        var gameQuartersDataTable = CreateQuartersDataTable(boxScores);
        var gameDidNotPlayDataTable = CreateDidNotPlayDataTable(boxScores);
        var playerBasicBoxScoreDataTable = CreatePlayerBasicBoxScoreDataTable(boxScores);
        var playerAdvancedBoxScoreDataTable = CreatePlayerAdvancedBoxScoreDataTable(boxScores);
        
        var parameters = new DynamicParameters();
        parameters.Add("@Games", gameBoxScoreDataTable.AsTableValuedParameter("nba.tt_Merge_GameBoxScore"));
        parameters.Add("@Quarters", gameQuartersDataTable.AsTableValuedParameter("nba.tt_Merge_GameQuarterScore"));
        parameters.Add("@DidNotPlay", gameDidNotPlayDataTable.AsTableValuedParameter("nba.tt_Merge_GameDidNotPlay"));
        parameters.Add("@PlayerBasicBoxScores", playerBasicBoxScoreDataTable.AsTableValuedParameter("nba.tt_Merge_GamePlayerBasicBoxScore"));
        parameters.Add("@PlayerAdvancedBoxScores", playerAdvancedBoxScoreDataTable.AsTableValuedParameter("nba.tt_Merge_GamePlayerAdvancedBoxScore"));
        
        await _dbConnection.ExecuteAsync("nba.sp_MergeGameBoxScores", parameters, commandType:CommandType.StoredProcedure);
    }
    
    private static DataTable CreateGameBoxScoreDataTable(List<GameBoxScore> boxScores)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("GameBoxScoreId", typeof(int));

        foreach (var gameOdds in boxScores)
            dt.Rows.Add(gameOdds.GameId);

        return dt;
    }

    private static DataTable CreateQuartersDataTable(List<GameBoxScore> boxScores)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("GameBoxScoreId", typeof(int));
        dt.Columns.Add("QuarterNumber", typeof(int));
        dt.Columns.Add("QuarterLabel", typeof(string));
        dt.Columns.Add("HomeScore", typeof(int));
        dt.Columns.Add("AwayScore", typeof(int));

        foreach (var boxScore in boxScores)
        {
            foreach (var quarter in boxScore.Quarters)
            {
                dt.Rows.Add(boxScore.GameId, quarter.QuarterNumber, quarter.QuarterLabel, quarter.HomeScore, quarter.AwayScore);            
            }            
        }

        return dt;
    }
    
    private static DataTable CreateDidNotPlayDataTable(List<GameBoxScore> boxScores)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("GameBoxScoreId", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("SeasonStartYear", typeof(int));
        dt.Columns.Add("PlayerName", typeof(string));
        dt.Columns.Add("Reason", typeof(string));

        foreach (var boxScore in boxScores)
        {
            foreach (var didNotPlay in boxScore.DidNotPlay)
            {
                dt.Rows.Add(boxScore.GameId, didNotPlay.TeamIdentifier, boxScore.Season.SeasonStartYear, didNotPlay.PlayerName, didNotPlay.Reason);            
            }            
        }

        return dt;
    }
    
    private static DataTable CreatePlayerBasicBoxScoreDataTable(List<GameBoxScore> boxScores)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("GameBoxScoreId", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("PlayerName", typeof(string));
        dt.Columns.Add("Starter", typeof(bool));
        dt.Columns.Add("SecondsPlayed", typeof(int));
        dt.Columns.Add("FieldGoalsMade", typeof(int));
        dt.Columns.Add("FieldGoalsAttempted", typeof(int));
        dt.Columns.Add("ThreePointersMade", typeof(int));
        dt.Columns.Add("ThreePointersAttempted", typeof(int));
        dt.Columns.Add("FreeThrowsMade", typeof(int));
        dt.Columns.Add("FreeThrowsAttempted", typeof(int));
        dt.Columns.Add("OffensiveRebounds", typeof(int));
        dt.Columns.Add("DefensiveRebounds", typeof(int));
        dt.Columns.Add("TotalRebounds", typeof(int));
        dt.Columns.Add("Assists", typeof(int));
        dt.Columns.Add("Steals", typeof(int));
        dt.Columns.Add("Blocks", typeof(int));
        dt.Columns.Add("Turnovers", typeof(int));
        dt.Columns.Add("PersonalFouls", typeof(int));
        dt.Columns.Add("Points", typeof(int));
        dt.Columns.Add("GameScore", typeof(decimal));
        dt.Columns.Add("PlusMinusScore", typeof(int));

        foreach (var boxScore in boxScores)
        {
            foreach (var quarter in boxScore.PlayerBasicBoxScore)
            {
                dt.Rows.Add(
                    boxScore.GameId, 
                    quarter.TeamIdentifier, 
                    quarter.PlayerName, 
                    quarter.Starter, 
                    quarter.SecondsPlayed,
                    quarter.FieldGoalsMade,
                    quarter.FieldGoalsAttempted,
                    quarter.ThreePointersMade,
                    quarter.ThreePointersAttempted,
                    quarter.FreeThrowsMade,
                    quarter.FreeThrowsAttempted,
                    quarter.OffensiveRebounds,
                    quarter.DefensiveRebounds,
                    quarter.TotalRebounds,
                    quarter.Assists,
                    quarter.Steals,
                    quarter.Blocks,
                    quarter.Turnovers,
                    quarter.PersonalFouls,
                    quarter.Points,
                    quarter.GameScore,
                    quarter.PlusMinusScore
                );            
            }            
        }

        return dt;
    }
    
    private static DataTable CreatePlayerAdvancedBoxScoreDataTable(List<GameBoxScore> boxScores)
    {
        var dt = new DataTable();
        
        dt.Columns.Add("GameBoxScoreId", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("PlayerName", typeof(string));
        dt.Columns.Add("TrueShootingPercentage", typeof(decimal));
        dt.Columns.Add("EffectiveFieldGoalPercentage", typeof(decimal));
        dt.Columns.Add("ThreePointAttemptRate", typeof(decimal));
        dt.Columns.Add("FreeThrowsAttemptRate", typeof(decimal));
        dt.Columns.Add("OffensiveReboundPercentage", typeof(decimal));
        dt.Columns.Add("DefensiveReboundPercentage", typeof(decimal));
        dt.Columns.Add("TotalReboundPercentage", typeof(decimal));
        dt.Columns.Add("AssistPercentage", typeof(decimal));
        dt.Columns.Add("StealPercentage", typeof(decimal));
        dt.Columns.Add("BlockPercentage", typeof(decimal));
        dt.Columns.Add("TurnoverPercentage", typeof(decimal));
        dt.Columns.Add("UsagePercentage", typeof(decimal));
        dt.Columns.Add("OffensiveRating", typeof(int));
        dt.Columns.Add("DefensiveRating", typeof(int));
        dt.Columns.Add("BoxPlusMinus", typeof(decimal));

        foreach (var boxScore in boxScores)
        {
            foreach (var quarter in boxScore.PlayerAdvancedBoxScore)
            {
                dt.Rows.Add(
                    boxScore.GameId, 
                    quarter.TeamIdentifier, 
                    quarter.PlayerName, 
                    quarter.TrueShootingPercentage, 
                    quarter.EffectiveFieldGoalPercentage,
                    quarter.ThreePointAttemptRate,
                    quarter.FreeThrowsAttemptRate,
                    quarter.OffensiveReboundPercentage,
                    quarter.DefensiveReboundPercentage,
                    quarter.TotalReboundPercentage,
                    quarter.AssistPercentage,
                    quarter.StealPercentage,
                    quarter.BlockPercentage,
                    quarter.TurnoverPercentage,
                    quarter.UsagePercentage,
                    quarter.OffensiveRating,
                    quarter.DefensiveRating,
                    quarter.BoxPlusMinus
                );            
            }            
        }

        return dt;
    }
}