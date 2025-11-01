using System;
using System.Collections.Generic;
using NbaOracle.Extensions;

namespace NbaOracle.ValueObjects;

public class Team : ValueObject
{
    public string Identifier { get; }
    public string Name { get; }

    public Team(string name, string identifier)
    {
        _ = name.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(name));
        _ = identifier.DiscardNullOrWhitespaceCheck() ?? throw new ArgumentNullException(nameof(identifier));

        Identifier = identifier.ToUpper();
        Name = name;
    }

    public override string ToString() => Identifier;

    public static string GetIdentifierByName(string name)
        => TeamsByName[name];

    public static string? GetIdentifierByNameOrNull(string name)
    {
        return TeamsByName.GetValueOrDefault(name);
    }
    
    private static readonly Dictionary<string, string> TeamsByKeyIdentifier = new()
    {
        { "ATL", "Atlanta Hawks" },
        { "BOS", "Boston Celtics" },
        { "BRK", "Brooklyn Nets" },
        { "CHA", "Charlotte Bobcats" },
        { "CHI", "Chicago Bulls" },
        { "CHO", "Charlotte Hornets" },
        { "CLE", "Cleveland Cavaliers" },
        { "DAL", "Dallas Mavericks" },
        { "DEN", "Denver Nuggets" },
        { "DET", "Detroit Pistons" },
        { "GSW", "Golden State Warriors" },
        { "HOU", "Houston Rockets" },
        { "IND", "Indiana Pacers" },
        { "LAC", "Los Angeles Clippers" },
        { "LAL", "Los Angeles Lakers" },
        { "MEM", "Memphis Grizzlies" },
        { "MIA", "Miami Heat" },
        { "MIL", "Milwaukee Bucks" },
        { "MIN", "Minnesota Timberwolves" },
        { "NOP", "New Orleans Pelicans" },
        { "NYK", "New York Knicks" },
        { "OKC", "Oklahoma City Thunder" },
        { "ORL", "Orlando Magic" },
        { "PHI", "Philadelphia 76ers" },
        { "PHO", "Phoenix Suns" },
        { "POR", "Portland Trail Blazers" },
        { "SAC", "Sacramento Kings" },
        { "SAS", "San Antonio Spurs" },
        { "TOR", "Toronto Raptors" },
        { "UTA", "Utah Jazz" },
        { "WAS", "Washington Wizards" }
    };

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
        { "Los Angeles Clippers", "LAC" },
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
        
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Identifier;
    }
}