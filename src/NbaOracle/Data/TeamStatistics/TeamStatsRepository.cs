using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.BasketballReference.Teams;

namespace NbaOracle.Data.TeamStatistics;

public class TeamStatisticsRepository
{
    private readonly IDbConnection _dbConnection;
        
    public TeamStatisticsRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task Merge(TeamStatisticsModel model)
    {
        var teamSeasonDataTable = CreateTeamSeasonDataTable(model);
        var playersDataTable = CreatePlayersDataTable(model);
        var teamRosterDataTable = CreateTeamRosterDataTable(model);
        var playerStatisticsDataTable = CreatePlayerStatisticsDataTable(model);
        
        var parameters = new DynamicParameters();
        parameters.Add("@Teams", teamSeasonDataTable.AsTableValuedParameter("nba.tt_Merge_TeamSeason"));
        parameters.Add("@Players", playersDataTable.AsTableValuedParameter("nba.tt_Merge_Player"));
        parameters.Add("@Roster", teamRosterDataTable.AsTableValuedParameter("nba.tt_Merge_TeamSeasonRoster"));
        parameters.Add("@PlayerStatistics", playerStatisticsDataTable.AsTableValuedParameter("nba.tt_Merge_PlayerSeasonStatistics"));
        
        await _dbConnection.ExecuteAsync("nba.sp_MergeTeamStatistics", parameters, commandType:CommandType.StoredProcedure);
    }
    
    private static DataTable CreateTeamSeasonDataTable(TeamStatisticsModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("SeasonStartYear", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("Wins", typeof(int));
        dt.Columns.Add("Losses", typeof(int));
        dt.Columns.Add("MarginOfVictory", typeof(decimal));
        dt.Columns.Add("OffensiveRating", typeof(decimal));
        dt.Columns.Add("DefensiveRating", typeof(decimal));
        dt.Columns.Add("WinsLeagueRank", typeof(int));
        dt.Columns.Add("LossesLeagueRank", typeof(int));
        dt.Columns.Add("MarginOfVictoryLeagueRank", typeof(int));
        dt.Columns.Add("OffensiveRatingLeagueRank", typeof(decimal));
        dt.Columns.Add("DefensiveRatingLeagueRank", typeof(decimal));
    
        var m = model.Misc;
        dt.Rows.Add(model.Season.SeasonStartYear, model.Team.Identifier, m.Wins, m.Losses, m.MarginOfVictory, m.OffensiveRating, m.DefensiveRating, m.WinsLeagueRank, m.LossesLeagueRank, m.MarginOfVictoryLeagueRank, m.OffensiveRatingLeagueRank, m.DefensiveRatingLeagueRank);

        return dt;
    }

    private static DataTable CreatePlayersDataTable(TeamStatisticsModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("Identifier", typeof(string));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("BirthDate", typeof(DateTime));
        dt.Columns.Add("BirthCountry", typeof(string));
        dt.Columns.Add("College", typeof(string));
    
        foreach (var player in model.Players)
            dt.Rows.Add(player.Identifier, player.Name, player.BirthDate, player.BirthCountry, player.College);

        return dt;
    }
    
    private static DataTable CreateTeamRosterDataTable(TeamStatisticsModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("SeasonStartYear", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("PlayerIdentifier", typeof(string));
        dt.Columns.Add("JerseyNumber", typeof(string));
        dt.Columns.Add("Position", typeof(string));
        dt.Columns.Add("Height", typeof(decimal));
        dt.Columns.Add("Weight", typeof(decimal));
        dt.Columns.Add("NumberOfYearInLeague", typeof(int));
    
        foreach (var player in model.Players)
            dt.Rows.Add(model.Season.SeasonStartYear, model.Team.Identifier, player.Identifier, player.JerseyNumber, player.Position, player.Height, player.Weight, player.NumberOfYearInLeague);

        return dt;
    }
    
    private static DataTable CreatePlayerStatisticsDataTable(TeamStatisticsModel model)
    {
        var dt = new DataTable();
            
        dt.Columns.Add("SeasonStartYear", typeof(int));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("PlayerIdentifier", typeof(string));
        dt.Columns.Add("ForRegularSeason", typeof(bool));
        dt.Columns.Add("GamesPlayed", typeof(int));
        dt.Columns.Add("MinutesPlayed", typeof(int));
        dt.Columns.Add("MinutedPlayedPerGame", typeof(decimal));
        dt.Columns.Add("FieldGoalsMade", typeof(int));
        dt.Columns.Add("FieldGoalsAttempted", typeof(int));
        dt.Columns.Add("FieldGoalPercentage", typeof(decimal));
        dt.Columns.Add("ThreePointersMade", typeof(int));
        dt.Columns.Add("ThreePointersAttempted", typeof(int));
        dt.Columns.Add("ThreePointersPercentage", typeof(decimal));
        dt.Columns.Add("TwoPointersMade", typeof(int));
        dt.Columns.Add("TwoPointersAttempted", typeof(int));
        dt.Columns.Add("TwoPointersPercentage", typeof(decimal));
        dt.Columns.Add("FreeThrowsMade", typeof(int));
        dt.Columns.Add("FreeThrowsAttempted", typeof(int));
        dt.Columns.Add("FreeThrowsPercentage", typeof(decimal));
        dt.Columns.Add("EffectiveFieldGoalPercentage", typeof(decimal));
        dt.Columns.Add("OffensiveRebounds", typeof(int));
        dt.Columns.Add("DefensiveRebounds", typeof(int));
        dt.Columns.Add("TotalRebounds", typeof(int));
        dt.Columns.Add("ReboundsPerGame", typeof(decimal));
        dt.Columns.Add("Assists", typeof(int));
        dt.Columns.Add("AssistsPerGame", typeof(decimal));
        dt.Columns.Add("Steals", typeof(int));
        dt.Columns.Add("StealsPerGame", typeof(decimal));
        dt.Columns.Add("Blocks", typeof(int));
        dt.Columns.Add("BlocksPerGame", typeof(decimal));
        dt.Columns.Add("Turnovers", typeof(int));
        dt.Columns.Add("TurnoversPerGame", typeof(decimal));
        dt.Columns.Add("PersonalFouls", typeof(int));
        dt.Columns.Add("PersonalFoulsPerGame", typeof(decimal));
        dt.Columns.Add("Points", typeof(int));
        dt.Columns.Add("PointsPerGame", typeof(decimal));
        dt.Columns.Add("PlusMinusOnCourt", typeof(decimal));
        dt.Columns.Add("PlusMinusNetOnCourt", typeof(decimal));

        foreach (var p in model.PlayerStatistics)
        {
            dt.Rows.Add(
                model.Season.SeasonStartYear,
                model.Team.Identifier,
                p.PlayerIdentifier,
                true,
                p.GamesPlayed,
                p.MinutesPlayed,
                p.MinutesPlayedPerGame,
                p.FieldGoalsMade,
                p.FieldGoalsAttempted,
                p.FieldGoalPercentage,
                p.ThreePointersMade,
                p.ThreePointersAttempted,
                p.ThreePointersPercentage,
                p.TwoPointersMade,
                p.TwoPointersAttempted,
                p.TwoPointersPercentage,
                p.FreeThrowsMade,
                p.FreeThrowsAttempted,
                p.FreeThrowsPercentage,
                p.EffectiveFieldGoalPercentage,
                p.OffensiveRebounds,
                p.DefensiveRebounds,
                p.TotalRebounds,
                p.ReboundsPerGame,
                p.Assists,
                p.AssistsPerGame,
                p.Steals,
                p.StealsPerGame,
                p.Blocks,
                p.BlocksPerGame,
                p.Turnovers,
                p.TurnoversPerGame,
                p.PersonalFouls,
                p.PersonalFoulsPerGame,
                p.Points,
                p.PointsPerGame,
                p.PlusMinusOnCourt,
                p.PlusMinusNetOnOffCourt
            );
        }

        return dt;
    }
}

public record TeamStatisticsModel(Team Team, Season Season, TeamMisc Misc, List<Player> Players, List<PlayerStatistics> PlayerStatistics);

public record TeamMisc(int Wins, int WinsLeagueRank, int Losses, int LossesLeagueRank, decimal MarginOfVictory, int MarginOfVictoryLeagueRank, decimal OffensiveRating, decimal OffensiveRatingLeagueRank, decimal DefensiveRating, decimal DefensiveRatingLeagueRank);

public record Player(string Identifier, string Name, DateTime BirthDate, string BirthCountry, string College, string JerseyNumber, string Position, decimal Height, decimal Weight, int NumberOfYearInLeague);

public record PlayerStatistics
{
    public string PlayerIdentifier { get; }
    public string PlayerName { get; }
    
    public int GamesPlayed { get; }
    public int MinutesPlayed { get; }
    public decimal MinutesPlayedPerGame { get; }
    
    public int FieldGoalsMade { get; }
    public int FieldGoalsAttempted { get; }
    public decimal FieldGoalPercentage { get; }

    public int ThreePointersMade { get; }
    public int ThreePointersAttempted { get; }
    public decimal ThreePointersPercentage { get; }

    public int TwoPointersMade { get; }
    public int TwoPointersAttempted { get; }
    public decimal TwoPointersPercentage { get; }

    public decimal EffectiveFieldGoalPercentage { get; }
    
    public int FreeThrowsMade { get; }
    public int FreeThrowsAttempted { get; }
    public decimal FreeThrowsPercentage { get; }

    public int OffensiveRebounds { get; }
    public int DefensiveRebounds { get; }
    public int TotalRebounds { get; }
    public decimal ReboundsPerGame { get; }

    public int Assists { get; }
    public decimal AssistsPerGame { get; }

    public int Steals { get; }
    public decimal StealsPerGame { get; }

    public int Blocks { get; }
    public decimal BlocksPerGame { get; }

    public int Turnovers { get; }
    public decimal TurnoversPerGame { get; }

    public int PersonalFouls { get; }
    public decimal PersonalFoulsPerGame { get; }

    public int Points { get; }
    public decimal PointsPerGame { get; }
    
    public decimal PlusMinusOnCourt { get; }
    public decimal PlusMinusNetOnOffCourt { get; }

    public PlayerStatistics(string playerIdentifier, string playerName, int gamesPlayed, int minutesPlayed, int fieldGoalsMade, int fieldGoalsAttempted, int threePointersMade, int threePointersAttempted, int twoPointersMade, int twoPointersAttempted, decimal effectiveFieldGoalPercentage, int freeThrowsMade, int freeThrowsAttempted, int offensiveRebounds, int defensiveRebounds, int assists, int  steals, int blocks, int turnovers, int personalFouls, int points, decimal plusMinusOnCourt, decimal plusMinusNetOnOffCourt)
    {
        PlayerIdentifier = playerIdentifier;
        PlayerName = playerName;
        
        GamesPlayed = gamesPlayed;
        MinutesPlayed = minutesPlayed;
        MinutesPlayedPerGame = Divide(minutesPlayed, gamesPlayed,1);
        
        FieldGoalsMade = fieldGoalsMade;
        FieldGoalsAttempted = fieldGoalsAttempted;
        FieldGoalPercentage = Divide(fieldGoalsMade, fieldGoalsAttempted);

        ThreePointersMade = threePointersMade;
        ThreePointersAttempted = threePointersAttempted;
        ThreePointersPercentage = Divide(threePointersMade, threePointersAttempted);

        TwoPointersMade = twoPointersMade;
        TwoPointersAttempted = twoPointersAttempted;
        TwoPointersPercentage = Divide(twoPointersMade, twoPointersAttempted);

        EffectiveFieldGoalPercentage = effectiveFieldGoalPercentage;
        
        FreeThrowsMade = freeThrowsMade;
        FreeThrowsAttempted = freeThrowsAttempted;
        FreeThrowsPercentage = Divide(freeThrowsMade, freeThrowsAttempted);
        
        OffensiveRebounds = offensiveRebounds;
        DefensiveRebounds = defensiveRebounds;
        TotalRebounds = offensiveRebounds + defensiveRebounds;
        ReboundsPerGame = Divide(TotalRebounds, gamesPlayed, 1);

        Assists = assists;
        AssistsPerGame = Divide(assists, gamesPlayed, 1);

        Steals = steals;
        StealsPerGame = Divide(steals, gamesPlayed, 1);

        Blocks = blocks;
        BlocksPerGame = Divide(blocks, gamesPlayed, 1);

        Turnovers = turnovers;
        TurnoversPerGame = Divide(turnovers, gamesPlayed, 1);

        PersonalFouls = personalFouls;
        PersonalFoulsPerGame = Divide(personalFouls, gamesPlayed, 1);

        Points = points;
        PointsPerGame = Divide(points, gamesPlayed, 1);

        PlusMinusOnCourt = plusMinusOnCourt;
        PlusMinusNetOnOffCourt = plusMinusNetOnOffCourt;
        
        return;

        static decimal Divide(int left, int right, int decimals = 3) => right == 0 ? 0 : Math.Round((decimal)left / right, decimals);
    }

    public override string ToString()
    {
        return $"{PlayerName} - ({Points} points)";
    }
}