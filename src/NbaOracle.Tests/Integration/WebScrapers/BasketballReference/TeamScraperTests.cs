using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.TeamStatistics;
using NbaOracle.ValueObjects;
using NbaOracle.WebScrapers.BasketballReference.Teams;
using Xunit;
using Xunit.Abstractions;

namespace NbaOracle.Tests.Integration.WebScrapers.BasketballReference;

public class TeamScraperTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public TeamScraperTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }
        
    [Theory]
    [InlineData(2025)]
    // [InlineData(2024)]
    // [InlineData(2023)]
    // [InlineData(2022)]
    // [InlineData(2021)]
    // [InlineData(2020)]
    // [InlineData(2019)]
    // [InlineData(2018)]
    // [InlineData(2017)]
    // [InlineData(2016)]
    // [InlineData(2015)]
    // [InlineData(2014)]
    // [InlineData(2013)]
    // [InlineData(2012)]
    // [InlineData(2011)]
    // [InlineData(2010)]
    // [InlineData(2009)]
    public async Task Scrape(int seasonStartYear)
    {
        var season = new Season(seasonStartYear);

        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
    
        var scraper = sp.GetRequiredService<TeamScraper>();

        var teams = TeamsFactory.GetTeamsBySeason(season);
        foreach (var team in teams)
        {
            var teamData = await scraper.Scrape(team, season);

            var teamInformation = TeamAdapter.Adapt(team, season, teamData);

            var repository = sp.GetRequiredService<TeamStatisticsRepository>();
            await repository.Merge(teamInformation);

            _output.WriteLine($"Processed team data - {team.Identifier} ({season.SeasonStartYear}-{season.SeasonEndYear})");
        }
    }
}