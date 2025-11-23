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
    
    [Fact]
    public async Task LightGbm_FewStrongFeatures()
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

        string[] features =
        [
            nameof(NbaGameFeatures.EloDiff),
         
            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
            
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
            
            nameof(NbaGameFeatures.RestDaysDiff),
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
        ];

        var options = new LightGbmBinaryTrainer.Options
        {
            NumberOfLeaves = 31,
            MinimumExampleCountPerLeaf = 20,
            LearningRate = 0.05,
            NumberOfIterations = 200,
            LabelColumnName = "Label",
            FeatureColumnName = "Features",
        };
        
        var lightGbm_CustomOptions = new LightGbmClassifier(options, ClassifierConfig.None, features);
        //var fastForest_WithMatchup_CustomOptions = new FastForestClassifier(options, ClassifierConfig.Matchup(CategoryConfig.OneHotHash(9)), features);
        //var fastForest_WithAllCategories_CustomOptions = new FastForestClassifier(options, ClassifierConfig.All(CategoryConfig.OneHot(), CategoryConfig.OneHot(), CategoryConfig.OneHotHash()), features);
        
        lightGbm_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        //fastForest_WithMatchup_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        //fastForest_WithAllCategories_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        
        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            (lightGbm_CustomOptions, new PredictionPerformanceTracker("LightGbm")),
            //(fastForest_WithMatchup_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With Matchup category")),
            //(fastForest_WithAllCategories_CustomOptions, new PredictionPerformanceTracker("FastForest - Custom With all categories"))
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
                
                    var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, game);
                    var awayEloProbability = 1 - homeEloProbability;
                    
                    var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
                    var awayGlickoProbability = 1 - homeGlickoProbability;
                    
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeEloMomentum5Games = (float) home.EloMomentum5Games,
                        AwayEloMomentum5Games = (float) away.EloMomentum5Games,
                        HomeEloMomentum10Games = (float) home.EloMomentum10Games,
                        AwayEloMomentum10Games = (float) away.EloMomentum10Games,
                        HomeEloProbability = (float) homeEloProbability,
                        AwayEloProbability = (float) awayEloProbability,
                        HomeGlickoRating = (float) home.GlickoScore.Rating,
                        AwayGlickoRating = (float) away.GlickoScore.Rating,
                        HomeGlickoRatingDeviation = (float) home.GlickoScore.RatingDeviation,
                        AwayGlickoRatingDeviation = (float) away.GlickoScore.RatingDeviation,
                        HomeGlickoVolatility = (float) home.GlickoScore.Volatility,
                        AwayGlickoVolatility = (float) away.GlickoScore.Volatility,
                        HomeGlickoProbability = (float) homeGlickoProbability,
                        AwayGlickoProbability = (float) awayGlickoProbability,
                        HomeOdds = (float?) gameOdds?.HomeOdds,
                        AwayOdds = (float?) gameOdds?.AwayOdds,
                        HomeTotalWinPercentage = (float) home.TotalWinPercentage,
                        AwayTotalWinPercentage = (float) away.TotalWinPercentage,
                        HomeWinPercentageAtHome = (float) home.HomeWinPercentage,
                        AwayWinPercentageWhenAway = (float) away.AwayWinPercentage,
                        HomeLastTenGamesWinPercentage = (float) home.LastTenGameWinPercentage,
                        AwayLastTenGamesWinPercentage = (float) away.LastTenGameWinPercentage,
                        HomeOffensiveRating = (float) home.LastTenGamesOffensiveRatingPercentage,
                        AwayOffensiveRating = (float) away.LastTenGamesOffensiveRatingPercentage,
                        HomeDefensiveRating = (float) home.LastTenGamesDefensiveRatingPercentage,
                        AwayDefensiveRating = (float) away.LastTenGamesDefensiveRatingPercentage,
                        HomeCurrentStreak = home.Streak,
                        AwayCurrentStreak = away.Streak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
    
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

        string[] features =
        [
            //nameof(NbaGameFeatures.OddsDiff),
            
            nameof(NbaGameFeatures.EloDiff),
            nameof(NbaGameFeatures.EloMomentum5GamesDiff),
            nameof(NbaGameFeatures.EloMomentum10GamesDiff),
            nameof(NbaGameFeatures.EloProbabilityDiff),
            
            nameof(NbaGameFeatures.RestDaysDiff),
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
            
            nameof(NbaGameFeatures.GlickoRatingDiff),
            nameof(NbaGameFeatures.GlickoRatingDeviationDiff),
            nameof(NbaGameFeatures.GlickoVolatilityDiff),
            nameof(NbaGameFeatures.GlickoProbabilityDiff),
            
            nameof(NbaGameFeatures.TotalWinPercentageDiff),
            
            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
            
            nameof(NbaGameFeatures.CurrentStreakDiff),
            nameof(NbaGameFeatures.LastTenGamesWinPercentageDiff),
            
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
        ];

        var options = new LightGbmBinaryTrainer.Options
        {
            NumberOfLeaves = 31,
            MinimumExampleCountPerLeaf = 20,
            LearningRate = 0.05,
            NumberOfIterations = 200,
            LabelColumnName = "Label",
            FeatureColumnName = "Features",
        };
        
        //var lightGbm_CustomOptions = new LightGbmClassifier(options, ClassifierConfig.None, features);
        var lightGbm_WithMatchup_CustomOptions = new LightGbmClassifier(options, ClassifierConfig.Matchup(CategoryConfig.OneHotHash(9)), features);
        
        //lightGbm_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        lightGbm_WithMatchup_CustomOptions.TrainModel(trainingDataSet.TrainingGames);
        
        var classifiers = new List<(IPredictionEngine, PredictionPerformanceTracker)>
        {
            //(lightGbm_CustomOptions, new PredictionPerformanceTracker("LightGbm")),
            (lightGbm_WithMatchup_CustomOptions, new PredictionPerformanceTracker("LightGbm - Custom With Matchup category")),
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
                
                    var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, game);
                    var awayEloProbability = 1 - homeEloProbability;
                    
                    var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
                    var awayGlickoProbability = 1 - homeGlickoProbability;
                    
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeEloMomentum5Games = (float) home.EloMomentum5Games,
                        AwayEloMomentum5Games = (float) away.EloMomentum5Games,
                        HomeEloMomentum10Games = (float) home.EloMomentum10Games,
                        AwayEloMomentum10Games = (float) away.EloMomentum10Games,
                        HomeEloProbability = (float) homeEloProbability,
                        AwayEloProbability = (float) awayEloProbability,
                        HomeGlickoRating = (float) home.GlickoScore.Rating,
                        AwayGlickoRating = (float) away.GlickoScore.Rating,
                        HomeGlickoRatingDeviation = (float) home.GlickoScore.RatingDeviation,
                        AwayGlickoRatingDeviation = (float) away.GlickoScore.RatingDeviation,
                        HomeGlickoVolatility = (float) home.GlickoScore.Volatility,
                        AwayGlickoVolatility = (float) away.GlickoScore.Volatility,
                        HomeGlickoProbability = (float) homeGlickoProbability,
                        AwayGlickoProbability = (float) awayGlickoProbability,
                        HomeOdds = (float?) gameOdds?.HomeOdds,
                        AwayOdds = (float?) gameOdds?.AwayOdds,
                        HomeTotalWinPercentage = (float) home.TotalWinPercentage,
                        AwayTotalWinPercentage = (float) away.TotalWinPercentage,
                        HomeWinPercentageAtHome = (float) home.HomeWinPercentage,
                        AwayWinPercentageWhenAway = (float) away.AwayWinPercentage,
                        HomeLastTenGamesWinPercentage = (float) home.LastTenGameWinPercentage,
                        AwayLastTenGamesWinPercentage = (float) away.LastTenGameWinPercentage,
                        HomeOffensiveRating = (float) home.LastTenGamesOffensiveRatingPercentage,
                        AwayOffensiveRating = (float) away.LastTenGamesOffensiveRatingPercentage,
                        HomeDefensiveRating = (float) home.LastTenGamesDefensiveRatingPercentage,
                        AwayDefensiveRating = (float) away.LastTenGamesDefensiveRatingPercentage,
                        HomeCurrentStreak = home.Streak,
                        AwayCurrentStreak = away.Streak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
    
    [Fact]
    public async Task FastForest_FewStrongFeatures()
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

        string[] features =
        [
            nameof(NbaGameFeatures.EloDiff),
         
            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
            
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
            
            nameof(NbaGameFeatures.RestDaysDiff),
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
        ];
        
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
        
        var fastForest_CustomOptions = new FastForestClassifier(options, ClassifierConfig.None, features);
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
            var model = historicalModel.Copy();
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var gameOdds = odds2024.GetValueOrDefault(game.GameId);
                    
                    var home = model.GetTeam(game.HomeTeam);
                    var away = model.GetTeam(game.AwayTeam);
                
                    var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, game);
                    var awayEloProbability = 1 - homeEloProbability;
                    
                    var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
                    var awayGlickoProbability = 1 - homeGlickoProbability;
                    
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeEloMomentum5Games = (float) home.EloMomentum5Games,
                        AwayEloMomentum5Games = (float) away.EloMomentum5Games,
                        HomeEloMomentum10Games = (float) home.EloMomentum10Games,
                        AwayEloMomentum10Games = (float) away.EloMomentum10Games,
                        HomeEloProbability = (float) homeEloProbability,
                        AwayEloProbability = (float) awayEloProbability,
                        HomeGlickoRating = (float) home.GlickoScore.Rating,
                        AwayGlickoRating = (float) away.GlickoScore.Rating,
                        HomeGlickoRatingDeviation = (float) home.GlickoScore.RatingDeviation,
                        AwayGlickoRatingDeviation = (float) away.GlickoScore.RatingDeviation,
                        HomeGlickoVolatility = (float) home.GlickoScore.Volatility,
                        AwayGlickoVolatility = (float) away.GlickoScore.Volatility,
                        HomeGlickoProbability = (float) homeGlickoProbability,
                        AwayGlickoProbability = (float) awayGlickoProbability,
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
                        HomeCurrentStreak = home.Streak,
                        AwayCurrentStreak = away.Streak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
    
    [Fact]
    public async Task FastForest_AllFeatures()
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

        string[] features =
        [
            // nameof(NbaGameFeatures.HomeEloRating),
            // nameof(NbaGameFeatures.AwayEloRating),
            nameof(NbaGameFeatures.EloDiff),
            
            // nameof(NbaGameFeatures.HomeEloMomentum5Games),
            // nameof(NbaGameFeatures.AwayEloMomentum5Games),
            nameof(NbaGameFeatures.EloMomentum5GamesDiff),
            
            // nameof(NbaGameFeatures.HomeEloMomentum10Games),
            // nameof(NbaGameFeatures.AwayEloMomentum10Games),
            nameof(NbaGameFeatures.EloMomentum10GamesDiff),
            
            // nameof(NbaGameFeatures.HomeEloProbability),
            // nameof(NbaGameFeatures.AwayEloProbability),
            nameof(NbaGameFeatures.EloProbabilityDiff),
            
            // nameof(NbaGameFeatures.HomeGlickoRating),
            // nameof(NbaGameFeatures.AwayGlickoRating),
            nameof(NbaGameFeatures.GlickoRatingDiff),
            
            // nameof(NbaGameFeatures.HomeGlickoRatingDeviation),
            // nameof(NbaGameFeatures.AwayGlickoRatingDeviation),
            nameof(NbaGameFeatures.GlickoRatingDeviationDiff),
            
            // nameof(NbaGameFeatures.HomeGlickoVolatility),
            // nameof(NbaGameFeatures.AwayGlickoVolatility),
            nameof(NbaGameFeatures.GlickoVolatilityDiff),
            
            // nameof(NbaGameFeatures.HomeGlickoProbability),
            // nameof(NbaGameFeatures.AwayGlickoProbability),
            nameof(NbaGameFeatures.GlickoProbabilityDiff),
            
            // nameof(NbaGameFeatures.HomeOdds),
            // nameof(NbaGameFeatures.AwayOdds),
            // nameof(NbaGameFeatures.OddsDiff),
            
            // nameof(NbaGameFeatures.HomeTotalWinPercentage),
            // nameof(NbaGameFeatures.AwayTotalWinPercentage),
            nameof(NbaGameFeatures.TotalWinPercentageDiff),
            
            nameof(NbaGameFeatures.HomeWinPercentageAtHome),
            nameof(NbaGameFeatures.AwayWinPercentageWhenAway),
            
            // nameof(NbaGameFeatures.HomeLastTenGamesWinPercentage),
            // nameof(NbaGameFeatures.AwayLastTenGamesWinPercentage),
            nameof(NbaGameFeatures.LastTenGamesWinPercentageDiff),
            
            // nameof(NbaGameFeatures.HomeOffensiveRating),
            // nameof(NbaGameFeatures.AwayOffensiveRating),
            nameof(NbaGameFeatures.OffensiveRatingDiff),
            
            // nameof(NbaGameFeatures.HomeDefensiveRating),
            // nameof(NbaGameFeatures.AwayDefensiveRating),
            nameof(NbaGameFeatures.DefensiveRatingDiff),
            
            // nameof(NbaGameFeatures.HomeCurrentStreak),
            // nameof(NbaGameFeatures.AwayCurrentStreak),
            nameof(NbaGameFeatures.CurrentStreakDiff),
            
            // nameof(NbaGameFeatures.HomeRestDays),
            // nameof(NbaGameFeatures.AwayRestDays),
            nameof(NbaGameFeatures.RestDaysDiff),
            
            nameof(NbaGameFeatures.HomeBackToBack),
            nameof(NbaGameFeatures.AwayBackToBack),
        ];

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
        
        var fastForest_CustomOptions = new FastForestClassifier(options, ClassifierConfig.None, features);
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
            var model = historicalModel.Copy();
            foreach (var dateGroup in games2024.GroupBy(x => x.GameDate))
            {
                foreach (var game in dateGroup)
                {
                    var gameOdds = odds2024.GetValueOrDefault(game.GameId);
                    
                    var home = model.GetTeam(game.HomeTeam);
                    var away = model.GetTeam(game.AwayTeam);
                
                    var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, game);
                    var awayEloProbability = 1 - homeEloProbability;
                    
                    var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
                    var awayGlickoProbability = 1 - homeGlickoProbability;
                    
                    var prediction = classifier.PredictGame(new NbaGameTrainingData
                    {
                        HomeIdentifier = home.TeamIdentifier,
                        AwayIdentifier = away.TeamIdentifier,
                        MatchupIdentifier = game.MatchupIdentifier,
                        HomeEloRating = (float) home.EloRating,
                        AwayEloRating = (float) away.EloRating,
                        HomeEloMomentum5Games = (float) home.EloMomentum5Games,
                        AwayEloMomentum5Games = (float) away.EloMomentum5Games,
                        HomeEloMomentum10Games = (float) home.EloMomentum10Games,
                        AwayEloMomentum10Games = (float) away.EloMomentum10Games,
                        HomeEloProbability = (float) homeEloProbability,
                        AwayEloProbability = (float) awayEloProbability,
                        HomeGlickoRating = (float) home.GlickoScore.Rating,
                        AwayGlickoRating = (float) away.GlickoScore.Rating,
                        HomeGlickoRatingDeviation = (float) home.GlickoScore.RatingDeviation,
                        AwayGlickoRatingDeviation = (float) away.GlickoScore.RatingDeviation,
                        HomeGlickoVolatility = (float) home.GlickoScore.Volatility,
                        AwayGlickoVolatility = (float) away.GlickoScore.Volatility,
                        HomeGlickoProbability = (float) homeGlickoProbability,
                        AwayGlickoProbability = (float) awayGlickoProbability,
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
                        HomeCurrentStreak = home.Streak,
                        AwayCurrentStreak = away.Streak,
                        HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
                        AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
                        HomeBackToBack = game.GameDate.AddDays(-1) == home.LastGameDate,
                        AwayBackToBack = game.GameDate.AddDays(-1) == away.LastGameDate,
                    });

                    var predictedWinner = prediction.HomeTeamWins ? game.HomeTeam : game.AwayTeam;
                    predictions.AddPrediction(new GamePredictionResult(game.GameId, game.WinTeam, predictedWinner, prediction.Probability));
                    
                    model.Evolve(new GameInfo(game, null));
                }   
            }
        }

        foreach (var (_, predictions) in classifiers)
        {
            _output.WriteLine($"{predictions.InstanceName} : ({predictions.CorrectPredictionCount}/{predictions.GamesCount}) - {predictions.PredictionAccuracy}");                
        }
    }
}