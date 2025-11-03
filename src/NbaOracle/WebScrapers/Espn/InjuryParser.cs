using System;
using System.Collections.Generic;
using System.Globalization;
using AngleSharp.Dom;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.Espn;

public class InjuryParser
{
    public static List<TeamInjuryReportData> Parse(IDocument document)
    {
        var teams = new List<TeamInjuryReportData>();
        
        var injuryTables = document.QuerySelectorAll("div.ResponsiveTable.Table__league-injuries");

        foreach (var injuryReport in injuryTables)
        {
            var teamNameSpan = injuryReport.QuerySelector("span.injuries__teamName.ml2");
            var teamName = teamNameSpan?.TextContent!;
            var team = Team.GetTeamByIdentifier(TeamsByName[teamName]);
            
            var rows = injuryReport.QuerySelectorAll("tr.Table__TR.Table__TR--sm.Table__even");

            var injuries = new List<PlayerInjuryData>();
            
            foreach (var row in rows)
            {
                var playerNameColumn = row.QuerySelector("td.col-name.Table__TD a.AnchorLink");
                var playerName = playerNameColumn?.TextContent!;
                
                var estimatedReturnColumn = row.QuerySelector("td.col-date.Table__TD");
                var estimatedReturnDateText = estimatedReturnColumn?.TextContent;
                var estimatedReturnDate = ParseInjuryDate(estimatedReturnDateText!);
                
                var statusColumn = row.QuerySelector("span.TextStatus");
                var status = statusColumn?.TextContent!;
                
                var descriptionColumn = row.QuerySelector("td.col-desc.Table__TD");
                var description = descriptionColumn?.TextContent!;
                
                injuries.Add(new PlayerInjuryData(playerName, estimatedReturnDate, status, description));
            }
            
            teams.Add(new TeamInjuryReportData(team, injuries));
        }

        return teams;
    }
    
    private static DateOnly ParseInjuryDate(string dateString)
    {
        var referenceDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-2);
    
        string[] formats = ["MMM d", "MMM dd"];

        if (!DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)) 
            throw new FormatException($"Unable to parse date: {dateString}");
        
        var date = new DateOnly(referenceDate.Year, parsedDate.Month, parsedDate.Day);
        
        if (date < referenceDate)
            date = date.AddYears(1);
        
        return date;

    }
    
    private static readonly Dictionary<string, string> TeamsByName = new()
    {
        { "Atlanta Hawks", "ATL" },
        { "Boston Celtics", "BOS" },
        { "Brooklyn Nets", "BRK" },
        { "Charlotte Bobcats", "CHA" },
        { "Charlotte Hornets", "CHO" },
        { "Chicago Bulls", "CHI" },
        { "Cleveland Cavaliers", "CLE" },
        { "Dallas Mavericks", "DAL" },
        { "Denver Nuggets", "DEN" },
        { "Detroit Pistons", "DET" },
        { "Golden State Warriors", "GSW" },
        { "Houston Rockets", "HOU" },
        { "Indiana Pacers", "IND" },
        { "LA Clippers", "LAC" },
        { "Los Angeles Lakers", "LAL" },
        { "Memphis Grizzlies", "MEM" },
        { "Miami Heat", "MIA" },
        { "Milwaukee Bucks", "MIL" },
        { "Minnesota Timberwolves", "MIN" },
        { "New Orleans Pelicans", "NOP" },
        { "New York Knicks", "NYK" },
        { "Oklahoma City Thunder", "OKC" },
        { "Orlando Magic", "ORL" },
        { "Philadelphia 76ers", "PHI" },
        { "Phoenix Suns", "PHO" },
        { "Portland Trail Blazers", "POR" },
        { "Sacramento Kings", "SAC" },
        { "San Antonio Spurs", "SAS" },
        { "Toronto Raptors", "TOR" },
        { "Utah Jazz", "UTA" },
        { "Washington Wizards", "WAS" }
    };
}