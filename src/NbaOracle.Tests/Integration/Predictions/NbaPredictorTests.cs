using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.Predictions;
using NbaOracle.Predictions.Classifiers;
using NbaOracle.Predictions.Elo;
using NbaOracle.Predictions.Glicko;
using NbaOracle.ValueObjects;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable InconsistentNaming

namespace NbaOracle.Tests.Integration.Predictions;

public class NbaPredictorTests : IntegrationTestBase
{ 
    private readonly ITestOutputHelper _output;

    public NbaPredictorTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    private static string[] StrongestFeatures =
    [
        nameof(NbaGameFeatures.EloDiff),
        nameof(NbaGameFeatures.RestDaysDiff),
        nameof(NbaGameFeatures.CurrentStreakDiff)
    ];
    
    private static string[] Features =
    [
        nameof(NbaGameFeatures.EloDiff),
        nameof(NbaGameFeatures.RestDaysDiff),
        nameof(NbaGameFeatures.CurrentStreakDiff),
        nameof(NbaGameFeatures.GamesLast7DaysDiff),
        
        // nameof(NbaGameFeatures.EloDiff),
        // nameof(NbaGameFeatures.EloMomentum5GamesDiff),
        // nameof(NbaGameFeatures.EloMomentum10GamesDiff),
        // nameof(NbaGameFeatures.EloProbabilityDiff),
        // nameof(NbaGameFeatures.GlickoRatingDiff),
        // nameof(NbaGameFeatures.GlickoRatingDeviationDiff),
        // nameof(NbaGameFeatures.GlickoVolatilityDiff),
        // nameof(NbaGameFeatures.GlickoProbabilityDiff),
        // nameof(NbaGameFeatures.CurrentStreakDiff),
        // nameof(NbaGameFeatures.TotalWinPercentageDiff),
        // nameof(NbaGameFeatures.HomeWinPercentageAtHome),
        // nameof(NbaGameFeatures.AwayWinPercentageWhenAway)
        // nameof(NbaGameFeatures.LastTenGamesWinPercentageDiff),
        // nameof(NbaGameFeatures.OffensiveRatingDiff),
        // nameof(NbaGameFeatures.DefensiveRatingDiff),
        // nameof(NbaGameFeatures.RestDaysDiff),
        // nameof(NbaGameFeatures.HomeBackToBack),
        // nameof(NbaGameFeatures.AwayBackToBack),
        // nameof(NbaGameFeatures.FourFactor10AvgPaceDiff),
        // nameof(NbaGameFeatures.FourFactor10AvgEfgDiff),
        // nameof(NbaGameFeatures.FourFactor10AvgTovDiff),
        // nameof(NbaGameFeatures.FourFactor10AvgOrbDiff),
        // nameof(NbaGameFeatures.FourFactor10AvgFtfgaDiff),
        // nameof(NbaGameFeatures.FourFactor10AvgOrtgDiff),
        // nameof(NbaGameFeatures.GamesLast7DaysDiff),
    ];
    
    [Fact]
    public async Task LightGbm_AllFeatures()
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var gameLoader = sp.GetRequiredService<GameLoader>();
        var oddsLoader = sp.GetRequiredService<GameBettingOddsLoader>();
        
        HashSet<string> teams =
        [
            "ATL", "BOS", "BRK", "CHO", "CHI", "CLE", "DAL", "DEN", "DET", "GSW", "HOU", "IND", "LAC", "LAL", "MEM",
            "MIA", "MIL", "MIN", "NOP", "NYK", "OKC", "ORL", "PHI", "PHO", "POR", "SAC", "SAS", "TOR", "UTA", "WAS"
        ];

        var historicalModel = new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), new Glicko2Calculator(), teams);
        
        int[] trainingSeasons = [2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023];
        var trainingDataSet = new TrainingDataSet();
        foreach (var season in trainingSeasons)
        {
            var games = await gameLoader.GetGames(new Season(season));
            var odds = (await oddsLoader.GetOdds(new Season(season))).ToDictionary(x => x.GameId, x => x);

            foreach (var game in games)
            {
                var gameInfo = new GameInfo(game, odds.GetValueOrDefault(game.GameId));
                trainingDataSet.AddGame(gameInfo, historicalModel);
                historicalModel.Evolve(gameInfo);
            }
            
            historicalModel.Regress();
        }

        var options = new LightGbmBinaryTrainer.Options
        {
            NumberOfLeaves = 31,
            MinimumExampleCountPerLeaf = 20,
            LearningRate = 0.05,
            NumberOfIterations = 200,
            LabelColumnName = "Label",
            FeatureColumnName = "Features",
        };
        
        var lightGbm_CustomOptions = new LightGbmClassifier(options, ClassifierConfig.None, Features);
        //var lightGbm_WithMatchup_CustomOptions = new LightGbmClassifier(options, ClassifierConfig.Matchup(CategoryConfig.OneHotHash(9)), features);
        
        lightGbm_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        //lightGbm_WithMatchup_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        
        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            (lightGbm_CustomOptions, new PredictionPerformanceTracker("LightGbm")),
            //(lightGbm_WithMatchup_CustomOptions, new PredictionPerformanceTracker("LightGbm - Custom With Matchup category")),
        };
        
        var games2024 = await gameLoader.GetGames(new Season(2024));
        var odds2024 = (await oddsLoader.GetOdds(new Season(2024))).ToDictionary(x => x.GameId, x => x);
        
        foreach (var (classifier, predictions) in classifiers)
        {
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var gameOdds = odds2024.GetValueOrDefault(game.GameId);
                    var trainingData = NbaGameTrainingDataFactory.Create(new GameInfo(game, gameOdds), historicalModel);
                    var prediction = classifier.PredictGame(trainingData);

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    historicalModel.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
    
    [Fact]
    public async Task FastForest()
    {
        await using var scope = CreateScope();
        var sp = scope.ServiceProvider;
        
        var gameLoader = sp.GetRequiredService<GameLoader>();
        var oddsLoader = sp.GetRequiredService<GameBettingOddsLoader>();
        
        HashSet<string> teams =
        [
            "ATL", "BOS", "BRK", "CHO", "CHI", "CLE", "DAL", "DEN", "DET", "GSW", "HOU", "IND", "LAC", "LAL", "MEM",
            "MIA", "MIL", "MIN", "NOP", "NYK", "OKC", "ORL", "PHI", "PHO", "POR", "SAC", "SAS", "TOR", "UTA", "WAS"
        ];

        var historicalModel = new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), new Glicko2Calculator(), teams);
        
        int[] trainingSeasons = [2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023];
        var trainingDataSet = new TrainingDataSet();
        foreach (var season in trainingSeasons)
        {
            var games = await gameLoader.GetGames(new Season(season));
            var odds = (await oddsLoader.GetOdds(new Season(season))).ToDictionary(x => x.GameId, x => x);

            foreach (var game in games)
            {
                var gameInfo = new GameInfo(game, odds.GetValueOrDefault(game.GameId));
                trainingDataSet.AddGame(gameInfo, historicalModel);
                historicalModel.Evolve(gameInfo);
            }
            
            historicalModel.Regress();
        }

        var options = new FastForestBinaryTrainer.Options
        {
            NumberOfTrees = 225,
            NumberOfLeaves = 55,
            MinimumExampleCountPerLeaf = 5,
            FeatureFraction = 0.68,
            FeatureFractionPerSplit = 0.78,   
            LabelColumnName = "Label",
            FeatureColumnName = "Features",
        };
        
        var fastForest_CustomOptions = new FastForestClassifier(options, ClassifierConfig.None, Features);
        //var fastForest_WithMatchup_CustomOptions = new FastForestClassifier(options, ClassifierConfig.Matchup(CategoryConfig.OneHotHash(9)), features);
        //var fastForest_WithAllCategories_CustomOptions = new FastForestClassifier(options, ClassifierConfig.All(CategoryConfig.OneHot(), CategoryConfig.OneHot(), CategoryConfig.OneHotHash()), features);
        
        fastForest_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        //fastForest_WithMatchup_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        //fastForest_WithAllCategories_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        
        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            (fastForest_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom")),
            //(fastForest_WithMatchup_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With Matchup category")),
            //(fastForest_WithAllCategories_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With all categories"))
        };
        
        var games2024 = await gameLoader.GetGames(new Season(2024));
        var odds2024 = (await oddsLoader.GetOdds(new Season(2024))).ToDictionary(x => x.GameId, x => x);
        
        foreach (var (classifier, predictions) in classifiers)
        {
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var gameOdds = odds2024.GetValueOrDefault(game.GameId);
                    var trainingData = NbaGameTrainingDataFactory.Create(new GameInfo(game, gameOdds), historicalModel);
                    var prediction = classifier.PredictGame(trainingData);

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    historicalModel.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
}