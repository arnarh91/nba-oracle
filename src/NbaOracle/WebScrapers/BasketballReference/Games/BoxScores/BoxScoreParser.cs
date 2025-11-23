using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NbaOracle.Extensions;
using NbaOracle.Infrastructure.AngleSharp;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;

public static class BoxScoreParser
{
    public static BoxScoreData Parse(IDocument document, string gameIdentifier)
    {
        var lineScore = ParseLineScore(document);
        var (fourFactorsHome, fourFactorsAway) = ParseFourFactors(document);
        var homeBoxScore = ParseBoxScore(document, lineScore.HomeTeam);
        var awayBoxScore = ParseBoxScore(document, lineScore.AwayTeam);

        homeBoxScore.FourFactors = fourFactorsHome;
        awayBoxScore.FourFactors = fourFactorsAway;
        
        return new BoxScoreData(gameIdentifier, lineScore, homeBoxScore, awayBoxScore);
    }

    private static LineScoreData ParseLineScore(IDocument document)
    {
        var lineScoreSelectorElement = document.QuerySelector("div[id='all_line_score']");

        var comment = lineScoreSelectorElement?.Descendants<IComment>().FirstOrDefault();
        if (comment == null)
            throw new InvalidOperationException("Unable to parse LineScore");

        var parser = new HtmlParser();
        var innerDocument = parser.ParseDocument(comment.TextContent.Trim());
        
        var lineScoreSelector = innerDocument.QuerySelectorAll("div[id='div_line_score'] tbody tr");
        
        var homeTeam = lineScoreSelector[1];
        var awayTeam = lineScoreSelector[0];
        
        var homeTeamIdentifier = homeTeam.GetTextContent("th[data-stat='team']");
        var awayTeamIdentifier = awayTeam.GetTextContent("th[data-stat='team']");

        var awayQ1 = awayTeam.GetTextContentAsInt("td[data-stat='1']");
        var awayQ2 = awayTeam.GetTextContentAsInt("td[data-stat='2']");
        var awayQ3 = awayTeam.GetTextContentAsInt("td[data-stat='3']");
        var awayQ4 = awayTeam.GetTextContentAsInt("td[data-stat='4']");

        var homeQ1 = homeTeam.GetTextContentAsInt("td[data-stat='1']");
        var homeQ2 = homeTeam.GetTextContentAsInt("td[data-stat='2']");
        var homeQ3 = homeTeam.GetTextContentAsInt("td[data-stat='3']");
        var homeQ4 = homeTeam.GetTextContentAsInt("td[data-stat='4']");

        var quarters = new List<QuarterScoreData> { new("1", homeQ1, awayQ1), new("2", homeQ2, awayQ2), new("3", homeQ3, awayQ3), new("4", homeQ4, awayQ4) };
        
        var overtimeCount = 1;
        while (true)
        {
            var overtimeHome = homeTeam.QuerySelector($"td[data-stat='{overtimeCount}OT']");
            if (overtimeHome is null)
                break;
            
            var overtimeAway = awayTeam.QuerySelector($"td[data-stat='{overtimeCount}OT']");
            if (overtimeAway is null)
                throw new InvalidOperationException("Unable to parse Overtime score");
            
            quarters.Add(new QuarterScoreData($"{overtimeCount}OT", ParsingMethods.ToInt(overtimeHome.TextContent), ParsingMethods.ToInt(overtimeAway.TextContent)));
            overtimeCount++;
        }
        
        return new LineScoreData(homeTeamIdentifier, awayTeamIdentifier, quarters);
    }

    private static (FourFactorsData Home, FourFactorsData Away) ParseFourFactors(IDocument document)
    {
        var fourFactorsElement = document.QuerySelector("div[id='all_four_factors']");

        var comment = fourFactorsElement?.Descendants<IComment>().FirstOrDefault();
        if (comment == null)
            throw new InvalidOperationException("Unable to parse FourFactors");
        
        var parser = new HtmlParser();
        var innerDocument = parser.ParseDocument(comment.TextContent.Trim());
        
        var fourFactorsSelector = innerDocument.QuerySelectorAll("div[id='div_four_factors'] tbody tr");
        
        var homeTeam = fourFactorsSelector[1];
        var awayTeam = fourFactorsSelector[0];

        var fourFactorsHome = new FourFactorsData
        (
            homeTeam.GetTextContentAsDecimal("td[data-stat='pace']"),
            homeTeam.GetTextContentAsDecimal("td[data-stat='efg_pct']"),
            homeTeam.GetTextContentAsDecimal("td[data-stat='tov_pct']"),
            homeTeam.GetTextContentAsDecimal("td[data-stat='orb_pct']"),
            homeTeam.GetTextContentAsDecimal("td[data-stat='ft_rate']"),
            homeTeam.GetTextContentAsDecimal("td[data-stat='off_rtg']")        
        );
        
        var fourFactorsAway = new FourFactorsData
        (
            awayTeam.GetTextContentAsDecimal("td[data-stat='pace']"),
            awayTeam.GetTextContentAsDecimal("td[data-stat='efg_pct']"),
            awayTeam.GetTextContentAsDecimal("td[data-stat='tov_pct']"),
            awayTeam.GetTextContentAsDecimal("td[data-stat='orb_pct']"),
            awayTeam.GetTextContentAsDecimal("td[data-stat='ft_rate']"),
            awayTeam.GetTextContentAsDecimal("td[data-stat='off_rtg']")        
        );

        return (fourFactorsHome, fourFactorsAway);
    }
        
    private static TeamBoxScoreData ParseBoxScore(IDocument document, string team)
    {
        var data = new TeamBoxScoreData();
        ParseBasicBoxScore(document, data, team);
        ParseAdvancedBoxScore(document, data, team);
        
        var inactivePlayers = ParseInactivePlayers(document, team);
        data.DidNotPlay.AddRange(inactivePlayers.Select(x => new DidNotPlayData(x, "inactive")));
        
        return data;
    }

    private static void ParseBasicBoxScore(IDocument document, TeamBoxScoreData data, string team)
    {
        var element = document.QuerySelector($"div#all_box-{team}-game-basic div#all_box-{team}-game-basic");
        if (element is null)
            throw new InvalidOperationException("Unable to parse basic box score");
        
        var players = element.QuerySelectorAll("tbody tr:not(.thead)");

        var i = 0;

        foreach (var player in players)
        {
            var playerName = player.GetTextContent("th[data-stat='player']");
            var starter = i++ < 5;
         
            var didNotPlay = player.GetTextContent("td[data-stat='reason']")?.ToLower(CultureInfo.InvariantCulture);
            if (didNotPlay is "did not play" or "did not dress" or "not with team" or "player suspended")
            {
                data.DidNotPlay.Add(new DidNotPlayData(playerName, didNotPlay));
                continue;
            }
        
            var minutesPlayed = new MinutesPlayedInGame(player.GetTextContent("td[data-stat='mp']"));
        
            var fieldGoalsMade = player.GetTextContentAsInt("td[data-stat='fg']");
            var fieldGoalsAttempted = player.GetTextContentAsInt("td[data-stat='fga']");
            
            var threePointersMade = player.GetTextContentAsInt("td[data-stat='fg3']");
            var threePointersAttempted = player.GetTextContentAsInt("td[data-stat='fg3a']");
         
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
         
            var plusMinusScore = player.GetTextContentAsInt("td[data-stat='plus_minus']");
         
            var gameScore = player.GetTextContentAsDecimal("td[data-stat='game_score']");
             
            data.BasicBoxScore.Add(new PlayerBasicBoxScoreData(
                playerName, 
                starter, 
                minutesPlayed.TotalSecondsPlayed(), 
                fieldGoalsMade, fieldGoalsAttempted, 
                threePointersMade, 
                threePointersAttempted, 
                freeThrowsMade, 
                freeThrowsAttempted, 
                offensiveRebounds, 
                defensiveRebounds, 
                assists, 
                steals, 
                blocks, 
                turnovers, 
                personalFouls, 
                points, 
                gameScore, 
                plusMinusScore)
            );
        }
    }

    private static List<string> ParseInactivePlayers(IDocument document, string team)
    {
        var inactivePlayersDiv = document.QuerySelectorAll("div").FirstOrDefault(d => d.Children.Any(c => c.TagName == "STRONG" && c.TextContent.Contains("Inactive:")));
        
        var nodes = inactivePlayersDiv!.ChildNodes.SkipWhile(n => !(n is IElement { TagName: "STRONG" } e && e.TextContent.Contains("Inactive:"))).Skip(1);

        var players = new List<string>();
        
        string currentTeam = null!;
    
        foreach (var node in nodes)
        {
            if (node is not IElement element) continue;
            switch (element.TagName)
            {
                case "SPAN":
                {
                    var strong = element.QuerySelector("strong");
                    if (strong != null)
                        currentTeam = strong.TextContent.Trim();

                    break;
                }
                case "A" when currentTeam != null && currentTeam == team:
                    var player = element.TextContent.Trim();
                    players.Add(player);
                    break;
            }
        }

        return players;
    }
    
    private static void ParseAdvancedBoxScore(IDocument document, TeamBoxScoreData data, string team)
    {
        var element = document.QuerySelector($"div#all_box-{team}-game-advanced div#all_box-{team}-game-advanced");
        if (element is null)
            throw new InvalidOperationException("Unable to parse advanced box score");
        
        var players = element.QuerySelectorAll("tbody tr:not(.thead)");

        foreach (var player in players)
        {
            var playerName = player.GetTextContent("th[data-stat='player']");
         
            var didNotPlay = player.GetTextContent("td[data-stat='reason']")?.ToLower(CultureInfo.InvariantCulture);
            if (didNotPlay is "did not play" or "did not dress" or "not with team" or "player suspended")
                continue;
        
            var trueShootingPercentage = player.GetTextContentAsDecimal("td[data-stat='ts_pct']");
            var effectiveFieldGoalPercentage = player.GetTextContentAsDecimal("td[data-stat='efg_pct']");
            var threePointAttemptRate = player.GetTextContentAsDecimal("td[data-stat='fg3a_per_fga_pct']");
            var freeThrowsAttemptRate = player.GetTextContentAsDecimal("td[data-stat='fta_per_fga_pct']");
            var offensiveReboundPercentage = player.GetTextContentAsDecimal("td[data-stat='orb_pct']");
            var defensiveReboundPercentage = player.GetTextContentAsDecimal("td[data-stat='drb_pct']");
            var totalReboundPercentage = player.GetTextContentAsDecimal("td[data-stat='trb_pct']");
            var assistPercentage = player.GetTextContentAsDecimal("td[data-stat='ast_pct']");
            var stealPercentage = player.GetTextContentAsDecimal("td[data-stat='stl_pct']");
            var blockPercentage = player.GetTextContentAsDecimal("td[data-stat='blk_pct']");
            var turnoverPercentage = player.GetTextContentAsDecimal("td[data-stat='tov_pct']");
            var usagePercentage = player.GetTextContentAsDecimal("td[data-stat='usg_pct']");
            var offensiveRating = player.GetTextContentAsInt("td[data-stat='off_rtg']");
            var defensiveRating = player.GetTextContentAsInt("td[data-stat='def_rtg']");
            var boxPlusMinus = player.GetTextContentAsDecimal("td[data-stat='bpm']");
             
            data.AdvancedBoxScore.Add(new PlayerAdvancedBoxScoreData(
                playerName,
                trueShootingPercentage,
                effectiveFieldGoalPercentage,
                threePointAttemptRate,
                freeThrowsAttemptRate,
                offensiveReboundPercentage,
                defensiveReboundPercentage,
                totalReboundPercentage,
                assistPercentage,
                stealPercentage,
                blockPercentage,
                turnoverPercentage,
                usagePercentage,
                offensiveRating,
                defensiveRating,
                boxPlusMinus
            ));
        }
    }
}