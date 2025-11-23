using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;

namespace NbaOracle.Data.Games;

public record Game
{
    public required int GameId { get; init; }
    public required string GameIdentifier { get; init; }
    public required DateOnly GameDate { get; init; }
    public required string HomeTeam { get; init; }
    public required string AwayTeam { get; init; }
    public required string WinTeam { get; init; }
    public required int HomePoints { get; init; }
    public required int AwayPoints { get; init; }
    public required string MatchupIdentifier { get; init; }
    public required string BoxScoreLink { get; init; }
    public TeamBoxScore HomeBoxScore { get; set; }
    public TeamBoxScore AwayBoxScore { get; set; }
}

public record TeamBoxScore
{
    public required FourFactors FourFactors { get; init; }
}

public record FourFactors
{
    public required decimal Pace { get; init; }
    public required decimal Efg { get; init; }
    public required decimal Tov { get; init; }
    public required decimal Orb { get; init; }
    public required decimal Ftfga { get; init; }
    public required decimal Ortg { get; init; }
}

public class GameLoader
{
    private readonly IDbConnection _dbConnection;

    public GameLoader(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task<List<Game>> GetGames(Season season)
    {
        await using var multiReader = await _dbConnection.QueryMultipleAsync("nba.sp_readSide_Games", new { startYear = season.SeasonStartYear }, commandType: CommandType.StoredProcedure);

        var games = (await multiReader.ReadAsync<Game>()).ToList();

        var boxScores = (await multiReader.ReadAsync<FourFactorsProjection>()).ToDictionary(x => new { x.GameBoxScoreId, x.TeamIdentifier}, x => x);
        
        foreach (var game in games)
        {
            if (!boxScores.TryGetValue(new { GameBoxScoreId = game.GameId, TeamIdentifier = game.HomeTeam }, out var homeBoxScore) || !boxScores.TryGetValue(new { GameBoxScoreId = game.GameId, TeamIdentifier = game.AwayTeam }, out var awayBoxScore))
                throw new InvalidOperationException("TeamBoxScore not found");
        
            game.HomeBoxScore = new TeamBoxScore
            {
                FourFactors = new FourFactors
                {
                    Pace = homeBoxScore.Pace,
                    Efg = homeBoxScore.Efg,
                    Tov = homeBoxScore.Tov,
                    Orb = homeBoxScore.Orb,
                    Ftfga = homeBoxScore.Ftfga,
                    Ortg = homeBoxScore.Ortg
                }
            };
            
            game.AwayBoxScore = new TeamBoxScore
            {
                FourFactors = new FourFactors
                {
                    Pace = awayBoxScore.Pace,
                    Efg = awayBoxScore.Efg,
                    Tov = awayBoxScore.Tov,
                    Orb = awayBoxScore.Orb,
                    Ftfga = awayBoxScore.Ftfga,
                    Ortg = awayBoxScore.Ortg
                }
            };
        }
        
        return games;
    }

    private record FourFactorsProjection
    {
        public required int GameBoxScoreId { get; init; }
        public required string TeamIdentifier { get; init; }
        public required decimal Pace { get; init; }
        public required decimal Efg { get; init; }
        public required decimal Tov { get; init; }
        public required decimal Orb { get; init; }
        public required decimal Ftfga { get; init; }
        public required decimal Ortg { get; init; }
    }
}