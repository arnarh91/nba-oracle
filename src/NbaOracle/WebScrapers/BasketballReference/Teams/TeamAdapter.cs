using System;
using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.TeamStatistics;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Teams;

public class TeamAdapter
{
    public static TeamStatisticsModel Adapt(Team team, Season season, TeamData teamData)
    {
        var m = teamData.Misc;
        
        var misc = new TeamMisc(m.Wins, m.WinsLeagueRank, m.Losses, m.LossesLeagueRank, m.MarginOfVictory, m.MarginOfVictoryLeagueRank, m.OffensiveRating, m.OffensiveRatingLeagueRank, m.DefensiveRating, m.DefensiveRatingLeagueRank);

        var playerStatistics = new List<PlayerStatistics>();

        var players = teamData.Roster.Select(x => new Player(x.GetPlayerIdentifier(), x.Name, x.BirthDate, x.BirthCountry, x.College, x.JerseyNumber, x.Position, x.Height, x.Weight, x.NumberOfYearInLeague)).ToList();
        
        foreach (var p in teamData.PlayerStatistics)
        {
            var player = players.Single(x => x.Name.Equals(p.PlayerName, StringComparison.InvariantCultureIgnoreCase));
            var playByPlay = teamData.PlayByPlay.Single(x => x.PlayerName.Equals(p.PlayerName, StringComparison.InvariantCultureIgnoreCase));
            
            playerStatistics.Add(new PlayerStatistics(
                player.Identifier,
                p.PlayerName,
                p.GamesPlayed,
                p.MinutesPlayed,
                p.FieldGoalsMade,
                p.FieldGoalsAttempted,
                p.ThreePointersMade,
                p.ThreePointersAttempted,
                p.TwoPointersMade,
                p.TwoPointersAttempted,
                p.EffectiveFieldGoalPercentage,
                p.FreeThrowsMade,
                p.FreeThrowsAttempted,
                p.OffensiveRebounds,
                p.DefensiveRebounds,
                p.Assists,
                p.Steals,
                p.Blocks,
                p.Turnovers,
                p.PersonalFouls,
                p.Points,
                playByPlay.PlusMinusOnCourt,
                playByPlay.PlusMinusNetOnOffCourt
            ));
        }
        
        return new TeamStatisticsModel(team, season, misc, players, playerStatistics);
    }
}