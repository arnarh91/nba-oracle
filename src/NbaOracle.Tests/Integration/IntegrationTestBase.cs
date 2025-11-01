using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using AngleSharp;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.Data.TeamStatistics;
using NbaOracle.Infrastructure.FileSystem;
using NbaOracle.Infrastructure.Http;
using NbaOracle.Infrastructure.Persistence;
using NbaOracle.WebScrapers.BasketballReference;
using NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;
using NbaOracle.WebScrapers.BasketballReference.Games.Results;
using NbaOracle.WebScrapers.BasketballReference.Teams;
using NbaOracle.WebScrapers.OddsPortal;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace NbaOracle.Tests.Integration
{
    public abstract class IntegrationTestBase
    {
        private readonly Container _container;

        protected IntegrationTestBase()
        {
            _container = new Container
            {
                Options = 
                { 
                    DefaultLifestyle = Lifestyle.Scoped, 
                    DefaultScopedLifestyle = new AsyncScopedLifestyle()
                }
            };

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettingsTest.json", optional: false, reloadOnChange: false)
                .Build();
            
            var baseDirectoryPath = configuration["BaseDirectoryPath"];
            var basketballReferenceBaseUrl = configuration["BasketballReferenceBaseUrl"];
            var oddsPortalBaseUrl = configuration["OddsPortalBaseUrl"];
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();

            serviceCollection.AddSingleton(new RateLimitingHandler(TimeSpan.FromSeconds(3)));
            
            serviceCollection.AddHttpClient<BasketballReferenceHttpClient>(client =>
            {
                client.BaseAddress = new Uri(basketballReferenceBaseUrl!);
                client.Timeout = TimeSpan.FromSeconds(30);
                
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            })
            .AddHttpMessageHandler<RateLimitingHandler>();

            // todo refactor this - drop simpleinjector
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            _container.RegisterSingleton<IFileSystem, FileSystem>();

            _container.Register<IDbConnection>(() => new SqlConnection("Database=nba;Server=localhost,14333;User ID=sa;Password=Password12;Trust Server Certificate=True;Encrypt=False"), Lifestyle.Scoped);

            _container.RegisterInstance(BrowsingContext.New(Configuration.Default));

            _container.Register<TeamScraper>();
            
            _container.RegisterInstance(new TeamOptions(basketballReferenceBaseUrl!, baseDirectoryPath!));
            
            _container.Register<TeamStatisticsRepository>();

            _container.RegisterInstance(new GameOptions(basketballReferenceBaseUrl, baseDirectoryPath));

            _container.Register<GameScraper>();
            _container.Register<GameRepository>();
            _container.Register<GameLoader>();
            
            _container.Register<GameBettingOddsRepository>();
            
            _container.RegisterInstance(new OddsPortalScraperOptions(oddsPortalBaseUrl!, baseDirectoryPath!));
            _container.Register<OddsPortalGameParser>();
            
            _container.Register<OddsPortalGameHtmlScraper>();
            
            _container.Register<GameBettingOddsLoader>();
            
            _container.Register<BoxScoreScraper>();
            
            _container.Register<GameBoxScoreRepository>();
            
            _container.Register(() => serviceProvider.GetRequiredService<BasketballReferenceHttpClient>(), Lifestyle.Scoped);
            
            _container.RegisterInstance(new BoxScoreOptions(baseDirectoryPath));
        }

        protected async Task ExecuteTest(Func<Container, Task> test)
        {
            await using (AsyncScopedLifestyle.BeginScope(_container))
            {
                await test(_container);
            }
        }
    }
}