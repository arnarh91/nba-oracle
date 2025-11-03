using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.OddsPortal;
using Xunit;

namespace NbaOracle.Tests.Integration.WebScrapers.OddsPortal;

public class OddsPortalGameParserTests : IntegrationTestBase
{
    [Theory]
    [InlineData(2024)]
    [InlineData(2023)]
    [InlineData(2022)]
    [InlineData(2021)]
    [InlineData(2020)]
    [InlineData(2019)]
    [InlineData(2018)]
    [InlineData(2017)]
    [InlineData(2016)]
    [InlineData(2015)]
    [InlineData(2014)]
    public async Task Scrape(int seasonStartYear)
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var season = new Season(seasonStartYear);
        
        var gameLoader = sp.GetRequiredService<GameLoader>(); 
        var scraper = sp.GetRequiredService<OddsPortalGameHtmlScraper>();
        var parser = sp.GetRequiredService<OddsPortalGameParser>();
        var repository = sp.GetRequiredService<GameBettingOddsRepository>();
        
        var games =  await gameLoader.GetGames(season);
        await scraper.Scrape(season);
        var oddsPortalGames = await parser.Parse(season);

        var adapter = new OddsPortalGameBettingOddsAdapter();
        var gameBettingOdds = adapter.Adapt(games, oddsPortalGames, DateTime.UtcNow);
        await repository.Merge(gameBettingOdds);
    }
}