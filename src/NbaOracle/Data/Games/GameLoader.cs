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
        const string sql = """
                           select g.Id GameId
                                , g.GameIdentifier
                                , g.GameDate
                                , g.HomeTeamIdentifier as HomeTeam
                                , g.AwayTeamIdentifier as AwayTeam
                                , g.WinTeamIdentifier as WinTeam
                                , g.HomePoints
                                , g.AwayPoints
                                , g.MatchupIdentifier
                                , g.BoxScoreLink
                           from   nba.Game g
                           join nba.Season s
                            on s.Id = g.SeasonId
                           where  s.StartYear = @startYear
                              and g.IsPlayoffGame = 0
                           order by g.GameDate;
                           """;

        return (await _dbConnection.QueryAsync<Game>(sql, new { startYear = season.SeasonStartYear }, commandType: CommandType.Text)).ToList();
    }
}