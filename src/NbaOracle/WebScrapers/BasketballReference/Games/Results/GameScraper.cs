using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.Results;

public class GameScraper
{
    private readonly BasketballReferenceHttpClient _httpClient;
    private readonly GameOptions _options; 
    private readonly IFileSystem _fileSystem;
    private readonly IBrowsingContext _browsingContext;
    
    public GameScraper(BasketballReferenceHttpClient httpClient, GameOptions options, IFileSystem fileSystem, IBrowsingContext browsingContext)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
    }
    
    public async Task<List<GameData>> Scrape(Season season, Month month)
    {
        var filePath = _options.GetFilePath(season, month);

        var htmlContent = await _fileSystem.GetFileContent(filePath);
        if (htmlContent is null)
        {
            htmlContent = await _httpClient.Get(_options.GetRequestUri(season, month));
            await _fileSystem.SaveFileContent(filePath, htmlContent);
        }

        var document = await _browsingContext.OpenAsync(request => { request.Content(htmlContent); });

        return GameParser.Parse(document);
    }
}