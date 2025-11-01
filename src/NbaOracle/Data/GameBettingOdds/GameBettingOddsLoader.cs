using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;

namespace NbaOracle.Data.GameBettingOdds;

public record GameBettingOdds
{
    public required int GameId { get; init; }
    public required decimal HomeOdds { get; init; }
    public required decimal AwayOdds { get; init; }
    public required decimal HomeConfidence { get; init; }
    public required decimal AwayConfidence { get; init; }
}

public class GameBettingOddsLoader
{
    private readonly IDbConnection _dbConnection;

    public GameBettingOddsLoader(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }
    
    public async Task<List<GameBettingOdds>> GetOdds(Season season)
    {
        const string sql = """
                           select   b.GameId
                                  , b.HomeOdds
                                  , b.AwayOdds
                                  , b.HomeConfidence
                                  , b.AwayConfidence
                           from     nba.GameBettingOdds b
                                    join nba.Game       g on g.Id = b.GameId
                           
                                    join nba.Season     s on s.Id = g.SeasonId
                           where    s.StartYear = @startYear
                                and g.IsPlayoffGame = 0
                           order by g.GameDate;
                           """;

        return (await _dbConnection.QueryAsync<GameBettingOdds>(sql, new { startYear = season.SeasonStartYear }, commandType: CommandType.Text)).ToList();
    }
}