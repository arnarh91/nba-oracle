using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;

public class BoxScoreOptions
{
    private readonly string _directoryPath;
    
    public BoxScoreOptions(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public string GetFilePath(BoxScoreLink boxScoreLink, Season season)
    {
        var directoryPath = $"{_directoryPath}/html/games/{season.SeasonStartYear}-{season.SeasonEndYear}/boxscores";
        return $"{directoryPath}/{boxScoreLink.BoxScoreId}.html";
    }
}