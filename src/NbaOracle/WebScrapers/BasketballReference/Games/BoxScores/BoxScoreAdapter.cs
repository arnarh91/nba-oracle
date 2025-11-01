using System;
using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;

public static class BoxScoreAdapter
{
    public static List<GameBoxScore> Adapt(List<BoxScoreData> boxScores, List<Game> games, Season season)
    {
        var map = boxScores.ToDictionary(x => x.GameIdentifier, x => x);

        var results = new List<GameBoxScore>(); 
        
        foreach (var game in games)
        {
            if (!map.TryGetValue(game.GameIdentifier, out var boxScore))
                throw new InvalidOperationException($"BoxScore was not found for game identifier '{game.GameIdentifier}'");

            var quarters = boxScore
                .LineScore
                .Quarters
                .Select(quarter => new GameQuarterScore(quarter.GetQuarterNumber(), quarter.Quarter, quarter.HomeScore, quarter.AwayScore)).ToList();

            var didNotPlay = new List<GameDidNotPlay>();
            didNotPlay.AddRange(boxScore.HomeBoxScore.DidNotPlay.Select(x => new GameDidNotPlay(boxScore.LineScore.HomeTeam, x.PlayerName, x.Reason)));
            didNotPlay.AddRange(boxScore.AwayBoxScore.DidNotPlay.Select(x => new GameDidNotPlay(boxScore.LineScore.AwayTeam, x.PlayerName, x.Reason)));

            var playerBasicBoxScore = new List<PlayerBasicBoxScore>();
            
            playerBasicBoxScore.AddRange(boxScore.HomeBoxScore.BasicBoxScore.Select(x =>
                new PlayerBasicBoxScore(x.PlayerName, boxScore.LineScore.HomeTeam, x.Starter, x.SecondsPlayed,
                    x.FieldGoalsMade, x.FieldGoalsAttempted, x.ThreePointersMade, x.ThreePointersAttempted,
                    x.FreeThrowsMade, x.FreeThrowsAttempted, x.OffensiveRebounds, x.DefensiveRebounds, x.Assists,
                    x.Steals, x.Blocks, x.Turnovers, x.PersonalFouls, x.Points, x.GameScore, x.PlusMinusScore)));
            
            playerBasicBoxScore.AddRange(boxScore.AwayBoxScore.BasicBoxScore.Select(x =>
                new PlayerBasicBoxScore(x.PlayerName, boxScore.LineScore.AwayTeam, x.Starter, x.SecondsPlayed,
                    x.FieldGoalsMade, x.FieldGoalsAttempted, x.ThreePointersMade, x.ThreePointersAttempted,
                    x.FreeThrowsMade, x.FreeThrowsAttempted, x.OffensiveRebounds, x.DefensiveRebounds, x.Assists,
                    x.Steals, x.Blocks, x.Turnovers, x.PersonalFouls, x.Points, x.GameScore, x.PlusMinusScore)));

            var playerAdvancedBoxScore = new List<PlayerAdvancedBoxScore>();
            
            playerAdvancedBoxScore.AddRange(boxScore.HomeBoxScore.AdvancedBoxScore.Select(x =>
                new PlayerAdvancedBoxScore(x.PlayerName, boxScore.LineScore.HomeTeam, x.TrueShootingPercentage, x.EffectiveFieldGoalPercentage,
                    x.ThreePointAttemptRate, x.FreeThrowsAttemptRate, Divide(x.OffensiveReboundPercentage), Divide(x.DefensiveReboundPercentage),
                    Divide(x.TotalReboundPercentage), Divide(x.AssistPercentage), Divide(x.StealPercentage), Divide(x.BlockPercentage), Divide(x.TurnoverPercentage),
                    Divide(x.UsagePercentage), x.OffensiveRating, x.DefensiveRating, x.BoxPlusMinus)));
            
            playerAdvancedBoxScore.AddRange(boxScore.AwayBoxScore.AdvancedBoxScore.Select(x =>
                new PlayerAdvancedBoxScore(x.PlayerName, boxScore.LineScore.AwayTeam, x.TrueShootingPercentage, x.EffectiveFieldGoalPercentage,
                    x.ThreePointAttemptRate, x.FreeThrowsAttemptRate, Divide(x.OffensiveReboundPercentage), Divide(x.DefensiveReboundPercentage),
                    Divide(x.TotalReboundPercentage), Divide(x.AssistPercentage), Divide(x.StealPercentage), Divide(x.BlockPercentage), Divide(x.TurnoverPercentage),
                    Divide(x.UsagePercentage), x.OffensiveRating, x.DefensiveRating, x.BoxPlusMinus)));
            
            var gameBoxScore = new GameBoxScore(game.GameId, season, quarters, didNotPlay, playerBasicBoxScore, playerAdvancedBoxScore);
            results.Add(gameBoxScore);
        }

        return results;

        decimal Divide(decimal number)
        {
            if (number == 0)
                return number;
            
            return number / 100.0m;
        }
    }
}