using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Teams;

public class TeamOptions
{
    public TeamOptions(string baseUrl, string directoryPath)
    {
        BaseUrl = $"{baseUrl}/teams";
        DirectoryPath = directoryPath;
    }

    private string BaseUrl { get; }
    private string DirectoryPath { get; }

    public string GetRequestUri(Team team, Season season)
    {
        return $"{BaseUrl}/{team.Identifier}/{season.SeasonEndYear}.html";
    }

    public string GetFilePath(Team team, Season season)
    {
        var directoryPath = $"{DirectoryPath}/html/teams/{season.SeasonStartYear}-{season.SeasonEndYear}";
        return $"{directoryPath}/{team.Identifier}.html";
    }
}