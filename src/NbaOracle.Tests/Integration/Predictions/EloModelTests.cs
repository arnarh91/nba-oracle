using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NbaOracle.Data.Games;
using NbaOracle.Predictions;
using NbaOracle.Predictions.Elo;
using NbaOracle.Predictions.Glicko;
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
        
        var models = new List<(NbaHistoricalModel, PredictionPerformanceTracker)>
        {
            (new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), new Glicko2Calculator(), teamIdentifiers), new PredictionPerformanceTracker("Season 2023/2024 & Margin with rest days (K=20, HomeAdvantage=80)")),
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
            foreach (var (model, prediction) in models)
            {
                var homeTeam = model.GetTeam(game.HomeTeam);
                var awayTeam = model.GetTeam(game.AwayTeam);
                    
                var homeWinProbability = model.EloCalculator.PredictWinProbability(homeTeam, awayTeam, game);
                //var homeWinProbability = model.GlickoCalculator.PredictWinProbability(homeTeam.GlickoScore, awayTeam.GlickoScore);
                var predictedWinner = homeWinProbability > 0.5 ? game.HomeTeam : game.AwayTeam;
                var confidence = game.HomeTeam == game.WinTeam ? homeWinProbability : 1 - homeWinProbability;
              
                prediction.AddPrediction(new GamePredictionResult(game.GameId, predictedWinner, game.WinTeam, confidence));
                
                model.Evolve(new GameInfo(game, null));
            }
        }
        
        foreach (var (_, prediction) in models)
        {
            _output.WriteLine($"{prediction.InstanceName} : ({prediction.CorrectPredictionCount}/{prediction.GamesCount}) - {prediction.PredictionAccuracy}");
        }
    }
    
    
}