using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.BasketballReference.Games.Results;
using Xunit;

namespace NbaOracle.Tests.Integration.Scrapers.BasketballReference;

public class GameScraperTests : IntegrationTestBase
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
    [InlineData(2013)]
    [InlineData(2012)]
    [InlineData(2011)]
    [InlineData(2010)]
    [InlineData(2009)]
    public async Task ProcessData(int seasonStartYear)
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var season = new Season(seasonStartYear);

        var scraper = sp.GetRequiredService<GameScraper>();
        var repository = sp.GetRequiredService<GameRepository>();

        var allGames = new List<GameData>();

        foreach (var month in season.GetMonthsInSeason())
        {
            var scrapedGames = await scraper.Scrape(season, month);

            allGames.AddRange(scrapedGames);
        }

        var games = GameAdapter.Adapt(allGames);
        
        await repository.Merge(season, games);
    }
}