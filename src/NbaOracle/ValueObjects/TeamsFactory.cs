using System.Collections.Generic;

namespace NbaOracle.ValueObjects;

public class TeamsFactory
{
    public static IReadOnlyList<Team> GetTeamsBySeason(Season season)
    {
        var teams = new List<Team>
        {
            new("Atlanta Hawks", "ATL"),
            new("Boston Celtics", "BOS"),
            new("Chicago Bulls", "CHI"),
            new("Cleveland Cavaliers", "CLE"),
            new("Dallas Mavericks", "DAL"),
            new("Denver Nuggets", "DEN"),
            new("Detroit Pistons", "DET"),
            new("Golden State Warriors", "GSW"),
            new("Houston Rockets", "HOU"),
            new("Indiana Pacers", "IND"),
            new("Los Angeles Clippers", "LAC"),
            new("Los Angeles Lakers", "LAL"),
            new("Memphis Grizzlies", "MEM"),
            new("Miami Heat", "MIA"),
            new("Milwaukee Bucks", "MIL"),
            new("Minnesota Timberwolves", "MIN"),
            new("New York Knicks", "NYK"),
            new("Oklahoma City Thunder", "OKC"),
            new("Orlando Magic", "ORL"),
            new("Philadelphia 76ers", "PHI"),
            new("Phoenix Suns", "PHO"),
            new("Portland Trail Blazers", "POR"),
            new("Sacramento Kings", "SAC"),
            new("San Antonio Spurs", "SAS"),
            new("Toronto Raptors", "TOR"),
            new("Utah Jazz", "UTA"),
            new("Washington Wizards", "WAS"),
            season.SeasonStartYear <= 2013 
                ? new Team("Charlotte Bobcats", "CHA") 
                : new Team("Charlotte Hornets", "CHO"), 
            season.SeasonStartYear <= 2012 
                ? new Team("New Orleans Hornets", "NOH") 
                : new Team("New Orleans Pelicans", "NOP"), 
            season.SeasonStartYear <= 2011  
                ? new Team("New Jersey Nets", "NJN")  
                : new Team("Brooklyn Nets", "BRK"), 
        };
            
        return teams;
    }
}