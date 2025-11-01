using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NbaOracle.Infrastructure.AngleSharp;

namespace NbaOracle.WebScrapers.BasketballReference.Teams;

public static class TeamParser
{
    public static TeamData Parse(IDocument document)
    {
        var misc = ParseMisc(document);
        var roster = ParseRoster(document);
        var playerStatistics = ParsePlayerStatistics(document);
        var playByPlay = ParsePlayByPlay(document);
        return new TeamData(misc, roster, playerStatistics, playByPlay);
    }

    private static TeamMiscData ParseMisc(IDocument document)
    {
        var teamMiscElement = document.QuerySelector("div[id='all_team_misc']");

        var comment = teamMiscElement?.Descendants<IComment>().FirstOrDefault();
        if (comment == null)
            throw new InvalidOperationException("Unable to parse TeamMisc");

        var parser = new HtmlParser();
        var innerDocument = parser.ParseDocument(comment.TextContent.Trim());
        
        var element = innerDocument.QuerySelectorAll("tbody tr");

        if (element.Length != 2)
            throw new InvalidOperationException("Unable to parse TeamMisc");

        var teamStats = element[0];
        var teamLeagueRanking = element[1];

        var wins = teamStats.GetTextContentAsInt("td[data-stat='wins']");
        var winsRank = teamLeagueRanking.GetTextContentAsInt("td[data-stat='wins']");

        var losses = teamStats.GetTextContentAsInt("td[data-stat='losses']");
        var lossesRank = teamLeagueRanking.GetTextContentAsInt("td[data-stat='losses']");

        var marginOfVictory = teamStats.GetTextContentAsDecimal("td[data-stat='mov']");
        var marginOfVictoryRank = teamLeagueRanking.GetTextContentAsInt("td[data-stat='mov']");
        
        var offensiveRating = teamStats.GetTextContentAsDecimal("td[data-stat='off_rtg']");
        var offensiveRatingRank = teamLeagueRanking.GetTextContentAsInt("td[data-stat='off_rtg']");
        
        var defensiveRating = teamStats.GetTextContentAsDecimal("td[data-stat='def_rtg']");
        var defensiveRatingRank = teamLeagueRanking.GetTextContentAsInt("td[data-stat='def_rtg']");

        return new TeamMiscData(wins, winsRank, losses, lossesRank, marginOfVictory, marginOfVictoryRank, offensiveRating, offensiveRatingRank, defensiveRating, defensiveRatingRank);
    }

    private static List<PlayerRosterData> ParseRoster(IDocument document)
    {
        var output = new List<PlayerRosterData>();

        foreach (var player in document.QuerySelectorAll("div[id='all_roster'] tbody tr"))
        {
            var playerName = player.GetTextContent("td[data-stat='player']");
            var playerNumber = player.GetTextContent("th[data-stat='number']");
            var position = player.GetTextContent("td[data-stat='pos']");
            var birthDate = player.GetAttributeFromElementAsDate("td[data-stat='birth_date']", "csk");
            var birthCountry = player.GetLastTextContent("td[data-stat='flag']");
            var height = player.GetTextContent("td[data-stat='height']");
            var weight = player.GetTextContent("td[data-stat='weight']");
            var yearsExperience = player.GetTextContent("td[data-stat='years_experience']");
            var college = player.GetTextContent("td[data-stat='college']");
            output.Add(PlayerRosterData.Create(playerName, playerNumber, position, birthDate, birthCountry, height, weight, yearsExperience, college));
        }
        
        return output;
    }
    
    private static List<PlayerSeasonStatisticsData> ParsePlayerStatistics(IDocument document)
    {
        var totalsElement = document.QuerySelector("div[id='all_totals_stats']");

        var comment = totalsElement?.Descendants<IComment>().FirstOrDefault();
        if (comment == null)
            throw new InvalidOperationException("Unable to parse PlayerStatistics");

        var parser = new HtmlParser();
        var innerDocument = parser.ParseDocument(comment.TextContent.Trim());
        
        var players = innerDocument.QuerySelectorAll("div#div_totals_stats tbody tr");
        var output = new List<PlayerSeasonStatisticsData>();
         
        foreach (var player in players)
        {
            var playerName = player.GetTextContent("td[data-stat='name_display']");

            var gamesPlayed = player.GetTextContentAsInt("td[data-stat='games']");
            var minutesPlayed = player.GetTextContentAsInt("td[data-stat='mp']");

            var fieldGoalsMade = player.GetTextContentAsInt("td[data-stat='fg']");
            var fieldGoalsAttempted = player.GetTextContentAsInt("td[data-stat='fga']");

            var threePointersMade = player.GetTextContentAsInt("td[data-stat='fg3']");
            var threePointersAttempted = player.GetTextContentAsInt("td[data-stat='fg3a']");

            var twoPointersMade = player.GetTextContentAsInt("td[data-stat='fg2']");
            var twoPointersAttempted = player.GetTextContentAsInt("td[data-stat='fg2a']");

            var effectiveFieldGoalPercentage = player.GetTextContentAsDecimal("td[data-stat='efg_pct']");

            var freeThrowsMade = player.GetTextContentAsInt("td[data-stat='ft']");
            var freeThrowsAttempted = player.GetTextContentAsInt("td[data-stat='fta']");

            var offensiveRebounds = player.GetTextContentAsInt("td[data-stat='orb']");
            var defensiveRebounds = player.GetTextContentAsInt("td[data-stat='drb']");

            var assists = player.GetTextContentAsInt("td[data-stat='ast']");
            var steals = player.GetTextContentAsInt("td[data-stat='stl']");
            var blocks = player.GetTextContentAsInt("td[data-stat='blk']");
            var turnovers = player.GetTextContentAsInt("td[data-stat='tov']");
            var personalFouls = player.GetTextContentAsInt("td[data-stat='pf']");
            var points = player.GetTextContentAsInt("td[data-stat='pts']");

            output.Add(new PlayerSeasonStatisticsData(playerName, gamesPlayed, minutesPlayed, fieldGoalsMade, fieldGoalsAttempted, threePointersMade, threePointersAttempted, twoPointersMade, twoPointersAttempted, effectiveFieldGoalPercentage, freeThrowsMade, freeThrowsAttempted, offensiveRebounds, defensiveRebounds, assists, steals, blocks, turnovers, personalFouls, points));
        }

        return output;
    }

    private static List<PlayByPlayData> ParsePlayByPlay(IDocument document)
    {
        var playByPLayElement = document.QuerySelector("div[id='all_pbp_stats']");

        var comment = playByPLayElement?.Descendants<IComment>().FirstOrDefault();
        if (comment == null)
            throw new InvalidOperationException("Unable to parse PlayerStatistics");

        var parser = new HtmlParser();
        var innerDocument = parser.ParseDocument(comment.TextContent.Trim());
        
        var players = innerDocument.QuerySelectorAll("div#div_pbp_stats tbody tr");

        var output = new List<PlayByPlayData>();

        foreach (var player in players)
        {
            var playerName = player.GetTextContent("td[data-stat='name_display']");
            var games = player.GetTextContentAsInt("td[data-stat='games']");
            var minutesPlayed = player.GetTextContentAsInt("td[data-stat='mp']");
            var plusMinusOnCourt = player.GetTextContent("td[data-stat='plus_minus_on']");
            var plusMinusNetOnCourt = player.GetTextContent("td[data-stat='plus_minus_net']");
            
            output.Add(PlayByPlayData.Create(playerName, games, minutesPlayed,plusMinusOnCourt, plusMinusNetOnCourt));
        }

        return output;
    }
}