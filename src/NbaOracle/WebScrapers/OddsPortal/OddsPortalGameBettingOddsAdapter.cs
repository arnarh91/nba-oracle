using System;
using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;

// ReSharper disable CollectionNeverQueried.Local

namespace NbaOracle.WebScrapers.OddsPortal;

public class OddsPortalGameBettingOddsAdapter
{
    public List<GameBettingOddsData> Adapt(List<Game> games, List<OddsPortalGameRecord> records, DateTime scrapedAt)
    {
        var missing = new List<Game>();
        var gameBettingOdds = new List<GameBettingOddsData>();

        foreach (var game in games)
        {
            var betGame = records.FirstOrDefault(x => x.HomeTeamIdentifier == game.HomeTeam && x.AwayTeamIdentifier == game.AwayTeam && (x.Date == game.GameDate || x.Date == game.GameDate.AddDays(1)));

            if (betGame is null || betGame.IsMissingOdds)
                missing.Add(game);
            else
                gameBettingOdds.Add(new GameBettingOddsData("OddsPortal", game.GameIdentifier, betGame.HomeTeamOdds!.Value, betGame.AwayTeamOdds!.Value, scrapedAt));
        }

        return gameBettingOdds;
    }
}