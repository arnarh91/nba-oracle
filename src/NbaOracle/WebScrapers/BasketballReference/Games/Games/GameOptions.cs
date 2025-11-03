using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.Games;

public class GameOptions
{
    public GameOptions(string baseUrl, string directoryPath)
    {
        BaseUrl = $"{baseUrl}/leagues";
        DirectoryPath = $"{directoryPath}/html/games";
    }

    private string BaseUrl { get; }
    private string DirectoryPath { get; }

    public string GetFilePath(Season season, Month month)
    {
        var directoryPath = $"{DirectoryPath}/{season.SeasonStartYear}-{season.SeasonEndYear}";
        return $"{directoryPath}/{month.Year}-{month.ToLower()}.html";
    }

    public string GetRequestUri(Season season, Month month)
    {
        return month.IsOctoberDuringEither2019Or2020()
            ? $"{BaseUrl}/NBA_{season.SeasonEndYear}_games-{month.ToLower()}-{month.Year}.html"
            : $"{BaseUrl}/NBA_{season.SeasonEndYear}_games-{month.ToLower()}.html";
    }
}