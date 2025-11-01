using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Trainers.FastTree;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.Predictions;
using NbaOracle.Predictions.Classifiers;
using NbaOracle.Predictions.Elo;
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
    
    [Fact]
    public async Task Predict_2024()
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

        var historicalModel = new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), teams);
        
        int[] trainingSeasons = [2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023];
        foreach (var season in trainingSeasons)
        {
            var games = await gameLoader.GetGames(new Season(season));
            var odds = (await oddsLoader.GetOdds(new Season(season))).ToDictionary(x => x.GameId, x => x);

            foreach (var game in games)
            {
                historicalModel.Process(new GameInfo(game, odds.GetValueOrDefault(game.GameId)));
            }
            
            historicalModel.Regress();
        }

        string[] features =
        [
            nameof(NbaGameFeatures.EloDiff),

            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
    
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
    
            nameof(NbaGameFeatures.HomeRestDays),
            nameof(NbaGameFeatures.AwayRestDays),
            nameof(NbaGameFeatures.RestDaysDiff),
    
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
        ];

        var trainingGames = historicalModel.TrainingGames;
        
        var lightGbm = new LightGbmClassifier(LightGbmClassifier.DefaultOptions, ClassifierConfig.None, features);
        var lightGbm_WithMatchup = new LightGbmClassifier(LightGbmClassifier.DefaultOptions, ClassifierConfig.Matchup(CategoryConfig.OneHotHash()), features);
        
        lightGbm.TrainModel(trainingGames);
        lightGbm_WithMatchup.TrainModel(trainingGames);
        
        var fastForest = new FastForestClassifier(FastForestClassifier.DefaultOptions, ClassifierConfig.None, features);
        var fastForest_WithMatchup = new FastForestClassifier(FastForestClassifier.DefaultOptions, ClassifierConfig.Matchup(CategoryConfig.OneHotHash()), features);
        
        fastForest.TrainModel(trainingGames);
        fastForest_WithMatchup.TrainModel(trainingGames);

        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            // (lightGbm, new PredictionPerformanceTracker("LightGbm ")),
            // (lightGbm_WithMatchup, new PredictionPerformanceTracker("LightGbm - With MatchupEncoding")),
            
            (fastForest, new PredictionPerformanceTracker("FastForest")),
            (fastForest_WithMatchup, new PredictionPerformanceTracker("FastForest - With MatchupEncoding"))
        };
        
        var games2024 = await gameLoader.GetGames(new Season(2024));

        foreach (var (classifier, predictions) in classifiers)
        {
            var model = historicalModel.Copy();
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var home = model.GetTeam(game.HomeTeam);
                    var away = model.GetTeam(game.AwayTeam);
                
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeTotalWinPercentage = (float) home.TotalWinPercentage,
                        AwayTotalWinPercentage = (float) away.TotalWinPercentage,
                        HomeWinPercentageAtHome = (float) home.HomeWinPercentage,
                        AwayWinPercentageWhenAway = (float) away.AwayWinPercentage,
                        HomeLastTenGamesWinPercentage = (float)home.LastTenGameWinPercentage,
                        AwayLastTenGamesWinPercentage = (float)away.LastTenGameWinPercentage,
                        HomeOffensiveRating = (float) home.LastTenGamesOffensiveRatingPercentage,
                        AwayOffensiveRating = (float) away.LastTenGamesOffensiveRatingPercentage,
                        HomeDefensiveRating = (float) home.LastTenGamesDefensiveRatingPercentage,
                        AwayDefensiveRating = (float) away.LastTenGamesDefensiveRatingPercentage,
                        HomeCurrentStreak = home.CurrentStreak,
                        AwayCurrentStreak = away.CurrentStreak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Process(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
    
    [Fact]
    public async Task OptimizeOptions()
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

        var historicalModel = new NbaHistoricalModel(new MarginWithRestDaysEloCalculator(20.0, 80.0, new RestDayConfiguration()), teams);
        
        int[] trainingSeasons = [2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023];
        foreach (var season in trainingSeasons)
        {
            var games = await gameLoader.GetGames(new Season(season));
            var odds = (await oddsLoader.GetOdds(new Season(season))).ToDictionary(x => x.GameId, x => x);

            foreach (var game in games)
            {
                historicalModel.Process(new GameInfo(game, odds.GetValueOrDefault(game.GameId)));
            }
            
            historicalModel.Regress();
        }

        // string[] features =
        // [
        //     nameof(NbaGameFeatures.EloDiff),
        //     nameof(NbaGameFeatures.OddsDiff),
        //
        //     nameof(NbaGameFeatures.HomeWinPercentageAtHome),
        //     nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
        //
        //     nameof(NbaGameFeatures.OffensiveRatingDiff),
        //     nameof(NbaGameFeatures.DefensiveRatingDiff),
        //
        //     nameof(NbaGameFeatures.HomeRestDays),
        //     nameof(NbaGameFeatures.AwayRestDays),
        //     nameof(NbaGameFeatures.RestDaysDiff),
        //
        //     nameof(NbaGameFeatures.HomeBackToBack),
        //     nameof(NbaGameFeatures.AwayBackToBack),
        // ];
        
        string[] features =
        [
            nameof(NbaGameFeatures.EloDiff),
            
            nameof(NbaGameFeatures.HomeRestDays),
            nameof(NbaGameFeatures.AwayRestDays),
            nameof(NbaGameFeatures.RestDaysDiff),
            
            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
    
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
    
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
        ];

        var trainingGames = historicalModel.TrainingGames;
        
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
        
        var fastForest_Default = new FastForestClassifier(FastForestClassifier.DefaultOptions, ClassifierConfig.None, features);
        //var fastForest_WithMatchup_Default = new FastForestClassifier(FastForestClassifier.DefaultOptions, ClassifierConfig.Matchup(CategoryConfig.OneHotHash()), features);
        
        var fastForest_CustomOptions = new FastForestClassifier(options, ClassifierConfig.None, features);
        //var fastForest_WithMatchup_CustomOptions = new FastForestClassifier(options, ClassifierConfig.Matchup(CategoryConfig.OneHotHash(9)), features);
        var fastForest_WithAllCategories_CustomOptions = new FastForestClassifier(options, ClassifierConfig.All(CategoryConfig.OneHot(), CategoryConfig.OneHot(), CategoryConfig.OneHotHash()), features);
        
        fastForest_Default.TrainModel(trainingGames);
        //fastForest_WithMatchup_Default.TrainModel(trainingGames);
        
        fastForest_CustomOptions.TrainModel(trainingGames);
        //fastForest_WithMatchup_CustomOptions.TrainModel(trainingGames);
        fastForest_WithAllCategories_CustomOptions.TrainModel(trainingGames);
        
        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            (fastForest_Default, new PredictionPerformanceTracker("FastForest - Default")),
            //(fastForest_WithMatchup_Default, new PredictionPerformanceTracker("FastForest - Default With Matchup category")),
            
            (fastForest_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom")),
            //(fastForest_WithMatchup_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With Matchup category")),
            //(fastForest_WithMatchup_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With all categories"))
        };
        
        var games2024 = await gameLoader.GetGames(new Season(2024));
        var odds2024 = (await oddsLoader.GetOdds(new Season(2024))).ToDictionary(x => x.GameId, x => x);
        
        foreach (var (classifier, predictions) in classifiers)
        {
            var model = historicalModel.Copy();
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var gameOdds = odds2024.GetValueOrDefault(game.GameId);
                    
                    var home = model.GetTeam(game.HomeTeam);
                    var away = model.GetTeam(game.AwayTeam);
                
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeOdds = (float?) gameOdds?.HomeOdds,
                        AwayOdds = (float?) gameOdds?.AwayOdds,
                        HomeTotalWinPercentage = (float) home.TotalWinPercentage,
                        AwayTotalWinPercentage = (float) away.TotalWinPercentage,
                        HomeWinPercentageAtHome = (float) home.HomeWinPercentage,
                        AwayWinPercentageWhenAway = (float) away.AwayWinPercentage,
                        HomeLastTenGamesWinPercentage = (float)home.LastTenGameWinPercentage,
                        AwayLastTenGamesWinPercentage = (float)away.LastTenGameWinPercentage,
                        HomeOffensiveRating = (float) home.LastTenGamesOffensiveRatingPercentage,
                        AwayOffensiveRating = (float) away.LastTenGamesOffensiveRatingPercentage,
                        HomeDefensiveRating = (float) home.LastTenGamesDefensiveRatingPercentage,
                        AwayDefensiveRating = (float) away.LastTenGamesDefensiveRatingPercentage,
                        HomeCurrentStreak = home.CurrentStreak,
                        AwayCurrentStreak = away.CurrentStreak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Process(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
}