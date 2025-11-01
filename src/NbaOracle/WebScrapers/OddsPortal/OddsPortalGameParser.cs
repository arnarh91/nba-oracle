using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using NbaOracle.Extensions;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.OddsPortal;

public class OddsPortalGameParser
{
    private readonly IBrowsingContext _browsingContext;
    private readonly IFileSystem _fileSystem;
    private readonly OddsPortalScraperOptions _options;

    public OddsPortalGameParser(IBrowsingContext browsingContext, IFileSystem fileSystem, OddsPortalScraperOptions options)
    {
        _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<List<OddsPortalGameRecord>> Parse(Season season)
    {
        var games = new List<OddsPortalGameRecord>();
        
        foreach (var page in OddsPortalUtilities.GetPageNumbersBySeason(season))
        {
            var filePath = _options.GetFilePath(season, page);
            var fileContent = await _fileSystem.GetFileContent(filePath);
            
            var document = await _browsingContext.OpenAsync(request => { request.Content(fileContent); });
            
            var eventRows = document.QuerySelectorAll("div.eventRow");

            var lastDate = DateOnly.MinValue;
            
            foreach (var eventRow in eventRows)
            {
                var dateElement = eventRow.QuerySelector("div[data-testid='date-header'] div");
                if (dateElement is not null)
                {
                    var datePart = dateElement!.InnerHtml.Split('-')[0].Trim();
                    lastDate = DateOnly.ParseExact(datePart, "dd MMM yyyy", CultureInfo.InvariantCulture);                    
                }
                
                if (lastDate == DateOnly.MinValue)
                    throw new InvalidOperationException("Unable to extract Date from results");

                var participants = eventRow.QuerySelectorAll("p.participant-name.truncate");
                if (participants.Length != 2)
                    throw new InvalidOperationException("Unable to extract team names from results");
                
                var homeTeamName = participants[0].TextContent.Trim().NormalizeName();
                var awayTeamName = participants[1].TextContent.Trim().NormalizeName();

                var homeTeamIdentifier = Team.GetIdentifierByNameOrNull(homeTeamName);
                var awayTeamIdentifier = Team.GetIdentifierByNameOrNull(awayTeamName);
                
                if (homeTeamIdentifier == null || awayTeamIdentifier == null)
                    continue;
                
                var odds = eventRow.QuerySelectorAll("p[data-testid^='odd-container']");

                if (odds.Length == 2)
                {
                    var hasHomeOdds = decimal.TryParse(odds[0].TextContent.Trim(), out var homeOdds);
                    var hasAwayOdds = decimal.TryParse(odds[1].TextContent.Trim(), out var awayOdds);
                    games.Add(new OddsPortalGameRecord(lastDate, homeTeamIdentifier, awayTeamIdentifier, hasHomeOdds ? homeOdds : null, hasAwayOdds ? awayOdds : null));
                }
                else 
                    games.Add(new OddsPortalGameRecord(lastDate, homeTeamName, awayTeamName, null, null)); 
            }
        }
        
        return games;
    }
}