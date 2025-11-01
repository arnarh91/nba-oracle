using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.OddsPortal;

public class OddsPortalScraperOptions
{
    public OddsPortalScraperOptions(string baseUrl, string directoryPath)
    {
        BaseUrl = $"{baseUrl}";
        DirectoryPath = $"{directoryPath}/html/betting-odds/";
    }

    private string BaseUrl { get; }
    private string DirectoryPath { get; }

    public string GetFilePath(Season season, int page)
    {
        var directoryPath = $"{DirectoryPath}{season.SeasonStartYear}-{season.SeasonEndYear}";
        return $"{directoryPath}/{season.SeasonStartYear}-{page}.html";
    }
    
    public string GetRequestUri(Season season, int page)
    {
        //https://www.oddsportal.com/basketball/usa/nba-2024-2025/results/#/page/1/
        return  $"{BaseUrl}/nba-{season.SeasonStartYear}-{season.SeasonEndYear}/results/#/page/{page}";
    }
}