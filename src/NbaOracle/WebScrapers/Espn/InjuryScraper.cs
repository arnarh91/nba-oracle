using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using NbaOracle.Infrastructure.FileSystem;

namespace NbaOracle.WebScrapers.Espn;

public class InjuriesScraper
{
    private readonly EspnHttpClient _httpClient;
    private readonly InjuryOptions _options;
    private readonly IBrowsingContext _browsingContext;
    private readonly IFileSystem _fileSystem;

    public InjuriesScraper(EspnHttpClient httpClient, InjuryOptions options, IBrowsingContext browsingContext, IFileSystem fileSystem)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<List<TeamInjuryReportData>> Scrape()
    {
        var filePath = _options.GetFilePath();

        string? htmlContent = null;
        
        if (!string.IsNullOrWhiteSpace(filePath))
            htmlContent = await _fileSystem.GetFileContent(filePath);

        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            htmlContent = await _httpClient.Get("injuries");
            if (!string.IsNullOrWhiteSpace(filePath))
                await _fileSystem.SaveFileContent(filePath, htmlContent);
        }
            
        var document = await _browsingContext.OpenAsync(request => { request.Content(htmlContent); });
        
        return InjuryParser.Parse(document);
    }
}

