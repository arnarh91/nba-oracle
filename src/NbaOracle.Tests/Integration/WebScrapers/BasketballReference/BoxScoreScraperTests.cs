using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;
using Xunit;

namespace NbaOracle.Tests.Integration.WebScrapers.BasketballReference;

public class BoxScoreScraperTests : IntegrationTestBase
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
    public async Task Provider(int seasonStartYear)
    {
        var season = new Season(seasonStartYear);
        
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
         
        var gameLoader = sp.GetRequiredService<GameLoader>();
        var boxScoreScraper = sp.GetRequiredService<BoxScoreScraper>();

        var games = await gameLoader.GetGames(season);

        var boxScores = new List<BoxScoreData>();
        foreach (var game in games)
        {
            boxScores.Add(await boxScoreScraper.Scrape(season, new BoxScoreLink(game.BoxScoreLink)));
        }

        var gameBoxScores = BoxScoreAdapter.Adapt(boxScores, games, season);

        var repository = sp.GetRequiredService<GameBoxScoreRepository>();

        await repository.Merge(gameBoxScores);
    }
}