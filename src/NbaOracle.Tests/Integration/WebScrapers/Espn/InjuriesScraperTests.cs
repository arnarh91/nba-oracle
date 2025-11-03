using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.Injuries;
using NbaOracle.WebScrapers.Espn;
using Xunit;

namespace NbaOracle.Tests.Integration.WebScrapers.Espn;

public class InjuriesScraperTests : IntegrationTestBase
{
    [Fact]
    public async Task Scrape()
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;

        var scraper = sp.GetRequiredService<InjuriesScraper>();
        var repository = sp.GetRequiredService<InjuryReportRepository>();

        var injuryReportData = await scraper.Scrape();

        var playerInjuries = InjuryAdapter.Adapt(injuryReportData);
        await repository.Merge(playerInjuries);
    }
}