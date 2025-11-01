using System;
using System.Threading.Tasks;
using AngleSharp;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;

public class BoxScoreScraper
{
    private readonly BasketballReferenceHttpClient _httpClient;
    private readonly BoxScoreOptions _options; 
    private readonly IFileSystem _fileSystem;
    private readonly IBrowsingContext _browsingContext;
    
    public BoxScoreScraper(BasketballReferenceHttpClient httpClient, BoxScoreOptions options, IFileSystem fileSystem, IBrowsingContext browsingContext)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
    }

    public async Task<BoxScoreData> Scrape(Season season, BoxScoreLink boxScoreLink)
    {
        var filePath = _options.GetFilePath(boxScoreLink, season);

        var htmlContent = await _fileSystem.GetFileContent(filePath);
        if (htmlContent is null)
        {
            htmlContent = await _httpClient.Get(boxScoreLink.ToHtmlLink());
            await _fileSystem.SaveFileContent(filePath, htmlContent);
        }

        var document = await _browsingContext.OpenAsync(request => { request.Content(htmlContent); });

        return BoxScoreParser.Parse(document, boxScoreLink.GameIdentifier);
    }
}