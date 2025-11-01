using System;
using System.Threading.Tasks;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.OddsPortal;
using Xunit;

namespace NbaOracle.Tests.Integration.Scrapers.OddsPortal;

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
        await ExecuteTest(async c =>
        {
            var season = new Season(seasonStartYear);
            
            var gameLoader = c.GetInstance<GameLoader>(); 
            var scraper = c.GetInstance<OddsPortalGameHtmlScraper>();
            var parser = c.GetInstance<OddsPortalGameParser>();
            var repository = c.GetInstance<GameBettingOddsRepository>();
            
            var games =  await gameLoader.GetGames(season);
            await scraper.Scrape(season);
            var oddsPortalGames = await parser.Parse(season);

            var adapter = new OddsPortalGameBettingOddsAdapter();
            var gameBettingOdds = adapter.Adapt(games, oddsPortalGames, DateTime.UtcNow);
            await repository.Merge(gameBettingOdds);
        });
    }
}