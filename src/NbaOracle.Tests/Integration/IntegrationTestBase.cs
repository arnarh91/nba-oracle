using System;
using System.Data;
using System.IO;
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

namespace NbaOracle.Tests.Integration;

public abstract class IntegrationTestBase
{
    private readonly IServiceProvider _serviceProvider;
    
    protected IntegrationTestBase()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .Build();
            
        var baseDirectoryPath = configuration["BaseDirectoryPath"];
        var basketballReferenceBaseUrl = configuration["BasketballReferenceBaseUrl"];
        var oddsPortalBaseUrl = configuration["OddsPortalBaseUrl"];

        if (string.IsNullOrWhiteSpace(baseDirectoryPath))
            throw new ArgumentException("BaseDirectoryPath needs to be configured");
        
        var serviceCollection = new ServiceCollection();
        
        // Infrastructure
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        
        serviceCollection.AddSingleton(new RateLimitingHandler(TimeSpan.FromSeconds(3)));
    
        serviceCollection.AddSingleton<IFileSystem, FileSystem>();
        serviceCollection.AddScoped<IDbConnection>(_ => new SqlConnection("Database=nba;Server=localhost,14333;User ID=sa;Password=Password12;Trust Server Certificate=True;Encrypt=False"));
        serviceCollection.AddSingleton(BrowsingContext.New(Configuration.Default));
        
        // WebScrapers
        serviceCollection.AddHttpClient<BasketballReferenceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(basketballReferenceBaseUrl!);
            client.Timeout = TimeSpan.FromSeconds(30);
        
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        })
        .AddHttpMessageHandler<RateLimitingHandler>();
        
        serviceCollection.AddSingleton(new TeamOptions(basketballReferenceBaseUrl!, baseDirectoryPath!));
        serviceCollection.AddScoped<TeamScraper>();
        
        serviceCollection.AddSingleton(new GameOptions(basketballReferenceBaseUrl, baseDirectoryPath));
        serviceCollection.AddScoped<GameScraper>();
        
        serviceCollection.AddSingleton(new OddsPortalScraperOptions(oddsPortalBaseUrl!, baseDirectoryPath!));
        serviceCollection.AddScoped<OddsPortalGameParser>();
        serviceCollection.AddScoped<OddsPortalGameHtmlScraper>();
        
        serviceCollection.AddSingleton(new BoxScoreOptions(baseDirectoryPath));
        serviceCollection.AddScoped<BoxScoreScraper>();
        
        // Persistence
        serviceCollection.AddScoped<TeamStatisticsRepository>();
        serviceCollection.AddScoped<GameRepository>();
        serviceCollection.AddScoped<GameBoxScoreRepository>();
        serviceCollection.AddScoped<GameBettingOddsRepository>();
        
        serviceCollection.AddScoped<GameLoader>();
        serviceCollection.AddScoped<GameBettingOddsLoader>();
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    protected AsyncServiceScope CreateScope()
    {
        return _serviceProvider.CreateAsyncScope();
    }
}