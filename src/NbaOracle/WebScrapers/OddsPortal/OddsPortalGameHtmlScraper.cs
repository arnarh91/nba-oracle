using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.ValueObjects;
using PuppeteerSharp;

namespace NbaOracle.WebScrapers.OddsPortal;

public class OddsPortalGameHtmlScraper
{
    private readonly IFileSystem _fileSystem;
    private readonly OddsPortalScraperOptions _options;

    public OddsPortalGameHtmlScraper(IFileSystem fileSystem, OddsPortalScraperOptions options)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task Scrape(Season season)
    {
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args =
            [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--window-size=1920,1080",
            ]
        };

        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        
        await using var browser = await Puppeteer.LaunchAsync(launchOptions);

        foreach (var pageNumber in OddsPortalUtilities.GetPageNumbersBySeason(season))
        {
            var filePath = _options.GetFilePath(season, pageNumber);

            var fileContent = await _fileSystem.GetFileContent(filePath); 
            if (!string.IsNullOrWhiteSpace(fileContent)) 
                continue;
            
            await Task.Delay(TimeSpan.FromSeconds(3));
            
            await using var page = await browser.NewPageAsync();

            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                ["Accept-Language"] = "en-US,en;q=0.9",
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
            });

            var requestUri = _options.GetRequestUri(season, pageNumber);
            await page.GoToAsync(requestUri, new NavigationOptions { Timeout = 15000 });
            await page.WaitForSelectorAsync("div[data-v-b18c49ad]", new WaitForSelectorOptions { Timeout = 15000 });

            // Naively scroll from top to bottom to trigger all the data being loaded to the dom before downloading the html
            await Task.Delay(2000);
            for (var i = 0; i < 5; i++)
            {
                await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight)");
                await Task.Delay(1000);
    
                await page.EvaluateExpressionAsync("window.scrollTo(0, 0)");
                await Task.Delay(1000);
            }
            await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight)");
            await Task.Delay(1000);
            await page.EvaluateExpressionAsync("window.scrollTo(0, 0)");
                
            var html = await page.GetContentAsync();

            await _fileSystem.SaveFileContent(filePath, html);
        }
    }
} // splitta upp download vs filesystem behavior - Eða má heita Historical? Og gera svo current sem skilar html? 