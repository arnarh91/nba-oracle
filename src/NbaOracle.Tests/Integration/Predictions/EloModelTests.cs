using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.Games;
using NbaOracle.Predictions;
using NbaOracle.Predictions.Elo;
using NbaOracle.ValueObjects;
using Xunit;
using Xunit.Abstractions;

namespace NbaOracle.Tests.Integration.Predictions;

public class EloModelTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public EloModelTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }
    
    [Fact]
    public async Task Train_2023_2024()
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var gameLoader = sp.GetRequiredService<GameLoader>();
        var games2023 = await gameLoader.GetGames(new Season(2023));
        var games2024 = await gameLoader.GetGames(new Season(2024));
        
        var teamIdentifiers = games2023.Select(x => x.HomeTeam).ToHashSet();

        /*
         * Only train only for the year 2024
         */
        var models = new List<(NbaHistoricalModel, PredictionPerformanceTracker)>
        {
            // // // Standard
            //  (new EloModel(new StandardEloCalculator(20.0), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & Standard (K=20)")),
            // //
            // // // HomeAdvantage
            // (new EloModel(new HomeAdvantageEloCalculator(20.0, 65.0), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & HomeAdvantage (K=20, HomeAdvantage=65)")),
            // (new EloModel(new HomeAdvantageEloCalculator(20.0, 80.0), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & HomeAdvantage (K=20, HomeAdvantage=80)")),
            // //
            // // // HomeAdvantage & Margin
            // (new EloModel(new HomeAdvantageWithMarginEloCalculator(20.0, 65.0), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & HomeAdvantage and margin (K=20, HomeAdvantage=65)")),
            // (new EloModel(new HomeAdvantageWithMarginEloCalculator(20.0, 80.0), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & HomeAdvantage and margin (K=20, HomeAdvantage=80)")),
            // //
            // // // Margin with rest days
            // (new EloModel(new MarginWithRestDaysEloCalculator(20.0, 65.0, new RestDayConfiguration()), teamIdentifiers, startDate2024), new PredictionPerformanceTracker("Season 2023/2024 & Margin with rest days (K=20, HomeAdvantage=65)")),
            (new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), teamIdentifiers), new PredictionPerformanceTracker("Season 2023/2024 & Margin with rest days (K=20, HomeAdvantage=80)")),
        };

        foreach (var game in games2023)
        {
            foreach (var (model, _) in models)
            {
                model.Evolve(new GameInfo(game, null));
            }
        }
        
        foreach (var (model, _) in models)
            model.Regress(regressionFactor:0.77);
        
        foreach (var game in games2024)
        {
            foreach (var (model, _) in models)
            {
                model.Evolve(new GameInfo(game, null));
            }
        }
        
        foreach (var game in games2024)
        {
            foreach (var (model, prediction) in models)
            {
                var homeRating = model.GetPreviousRating(game.HomeTeam, game.GameDate);
                var awayRating = model.GetPreviousRating(game.AwayTeam, game.GameDate);
                
                string predictedWinner;
                if (homeRating.Rating > awayRating.Rating)
                    predictedWinner = game.HomeTeam;
                else if (homeRating.Rating < awayRating.Rating)
                    predictedWinner = game.AwayTeam;
                else
                    predictedWinner = game.HomeTeam;
                
                prediction.AddPrediction(new GamePredictionResult(game.GameId, predictedWinner, game.WinTeam, null));
            }
        }
        
        foreach (var (_, prediction) in models)
        {
            _output.WriteLine($"{prediction.InstanceName} : ({prediction.CorrectPredictionCount}/{prediction.GamesCount}) - {prediction.PredictionAccuracy}");
        }
    }
    
    [Fact]
    public async Task Optimize()
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var gameLoader = sp.GetRequiredService<GameLoader>();
        var games2023 = await gameLoader.GetGames(new Season(2023));
        var games2024 = await gameLoader.GetGames(new Season(2024));
        
        var teamIdentifiers = games2023.Select(x => x.HomeTeam).ToHashSet();

        /*
         * Only train only for the year 2024
         */
        var models = new List<(NbaHistoricalModel, PredictionPerformanceTracker)>
        {
            (new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), teamIdentifiers), new PredictionPerformanceTracker("Season 2023/2024 & Margin with rest days (K=20, HomeAdvantage=80)")),
        };

        foreach (var game in games2023)
        {
            foreach (var (model, _) in models)
            {
                model.Evolve(new GameInfo(game, null));
            }
        }
        
        foreach (var (model, _) in models)
            model.Regress(regressionFactor:0.77);
        
        foreach (var game in games2024)
        {
            foreach (var (model, _) in models)
            {
                model.Evolve(new GameInfo(game, null));
            }
        }
        
        foreach (var game in games2024)
        {
            foreach (var (model, prediction) in models)
            {
                var homeRating = model.GetPreviousRating(game.HomeTeam, game.GameDate);
                var awayRating = model.GetPreviousRating(game.AwayTeam, game.GameDate);
                
                string predictedWinner;

                if (homeRating.Rating > awayRating.Rating)
                    predictedWinner = game.HomeTeam;
                else if (homeRating.Rating < awayRating.Rating)
                    predictedWinner = game.AwayTeam;
                else
                    predictedWinner = game.HomeTeam;

                prediction.AddPrediction(new GamePredictionResult(game.GameId, predictedWinner, game.WinTeam, null));
            }
        }
        
        foreach (var (_, prediction) in models)
        {
            _output.WriteLine($"{prediction.InstanceName} : ({prediction.CorrectPredictionCount}/{prediction.GamesCount}) - {prediction.PredictionAccuracy}");
        }
    }
}