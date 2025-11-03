using System;
using System.Threading.Tasks;
using AngleSharp;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Teams;

public class TeamScraper
{
    private readonly BasketballReferenceHttpClient _httpClient;
    private readonly TeamOptions _options; 
    private readonly IFileSystem _fileSystem;
    private readonly IBrowsingContext _browsingContext;

    public TeamScraper(BasketballReferenceHttpClient httpClient, TeamOptions options, IFileSystem fileSystem, IBrowsingContext browsingContext)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
    }

    public async Task<TeamData> Scrape(Team team, Season season)
    {
        var shouldCache = !season.IsCurrentSeason(DateTime.Today);
        string? htmlContent;
        if (shouldCache)
        {
            var filePath = _options.GetFilePath(team, season);

            htmlContent = await _fileSystem.GetFileContent(filePath);
            if (htmlContent is null)
            {
                htmlContent = await _httpClient.Get(_options.GetRequestUri(team, season));
                await _fileSystem.SaveFileContent(filePath, htmlContent);
            }            
        }
        else 
            htmlContent = await _httpClient.Get(_options.GetRequestUri(team, season));
        
        var document = await _browsingContext.OpenAsync(request => { request.Content(htmlContent); });

        return TeamParser.Parse(document);
    }    
}