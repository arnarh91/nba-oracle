using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.Games;

namespace NbaOracle.WebScrapers.BasketballReference.Games.Games;

public class GameAdapter
{
    public static List<GameModel> Adapt(List<GameData> games)
    {
        return games.Select(x => new GameModel(x.GameDate, x.AwayTeam, x.HomeTeam, x.AwayPoints, x.HomePoints, x.BoxScoreLink, x.NumberOfOvertimes, x.Attendance, x.IsPlayoffGame)).ToList();
    }
}