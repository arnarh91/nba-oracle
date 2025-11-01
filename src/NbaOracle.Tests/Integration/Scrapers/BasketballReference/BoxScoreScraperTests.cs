using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NbaOracle.Data.Games;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;
using Xunit;

namespace NbaOracle.Tests.Integration.Scrapers.BasketballReference;

public class BoxScoreScraperTests : IntegrationTestBase
{
    [Theory]
    //[InlineData(2024)]
    [InlineData(2023)]
    public async Task Provider(int seasonStartYear)
    {
        var season = new Season(seasonStartYear);
            
        await ExecuteTest(async c =>
        {
            var gameLoader = c.GetInstance<GameLoader>();
            var boxScoreScraper = c.GetInstance<BoxScoreScraper>();

            var games = await gameLoader.GetGames(season);
            //games = games.Take(1).ToList();

            var boxScores = new List<BoxScoreData>();
            foreach (var game in games)
            {
                boxScores.Add(await boxScoreScraper.Scrape(season, new BoxScoreLink(game.BoxScoreLink)));
            }

            var gameBoxScores = BoxScoreAdapter.Adapt(boxScores, games, season);

            var repository = c.GetInstance<GameBoxScoreRepository>();

            await repository.Merge(gameBoxScores);
        });
    }
}