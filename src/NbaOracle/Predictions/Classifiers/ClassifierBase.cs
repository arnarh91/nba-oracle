using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using NbaOracle.Predictions.Elo;

// ReSharper disable ConvertToPrimaryConstructor

namespace NbaOracle.Predictions.Classifiers;

public interface IPredictionEngine
{
    NbaGamePrediction PredictGame(NbaGameTrainingData gameTrainingData);
}

public abstract class ClassifierBase<TOptions> : IPredictionEngine 
    where TOptions : TrainerInputBase
{
    private readonly TOptions _options;
    private readonly ClassifierConfig _classifierConfig;
    private readonly string[] _features;
 
    private readonly MLContext _mlContext;
    private ITransformer _model = null!;
    private PredictionEngine<NbaGameFeatures, NbaGamePrediction> _predictionEngine = null!;
    
    protected ClassifierBase(TOptions options, ClassifierConfig classifierConfig, string[] features)
    {
        _options = options;
        _features = features;
        _classifierConfig = classifierConfig;
        _mlContext = new MLContext(classifierConfig.Seed);
    }

    protected abstract IEstimator<ITransformer> CreateTrainer(MLContext mlContext, TOptions options);
    
    public void TrainModel(IEnumerable<NbaGameTrainingData> trainingGames)
    {
        var trainingFeatures = ConvertToFeatures(trainingGames);
        var trainingData = _mlContext.Data.LoadFromEnumerable(trainingFeatures);

        var pipeline = BuildPipeline();
        
        _model = pipeline.Fit(trainingData);
        
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<NbaGameFeatures, NbaGamePrediction>(_model);
    }

    public NbaGamePrediction PredictGame(NbaGameTrainingData gameTrainingData)
    {
        if (_predictionEngine == null)
            throw new InvalidOperationException("Model has not been trained yet. Call TrainModel first.");

        var features = ConvertToFeatures([gameTrainingData]).First();
        return _predictionEngine.Predict(features);
    }
    
    private static IEnumerable<NbaGameFeatures> ConvertToFeatures(IEnumerable<NbaGameTrainingData> games)
    {
        return games.Select(game => new NbaGameFeatures
        {
            HomeTeamIdentifier = game.HomeIdentifier,
            AwayTeamIdentifier = game.AwayIdentifier,
            MatchupIdentifier = game.MatchupIdentifier,
            HomeTeamWon = game.HomeTeamWon,
            
            EloDiff = game.HomeEloRating - game.AwayEloRating,
            HomeEloRating = game.HomeEloRating,
            AwayEloRating = game.AwayEloRating,
            
            HomeEloMomentum5Games = game.HomeEloMomentum5Games,
            AwayEloMomentum5Games = game.AwayEloMomentum5Games,
            EloMomentum5GamesDiff = game.HomeEloMomentum5Games - game.AwayEloMomentum5Games,
            
            HomeEloMomentum10Games = game.HomeEloMomentum10Games,
            AwayEloMomentum10Games = game.AwayEloMomentum10Games,
            EloMomentum10GamesDiff = game.HomeEloMomentum10Games - game.AwayEloMomentum10Games,
            
            HomeEloProbability = game.HomeEloProbability,
            AwayEloProbability = game.AwayEloProbability,
            EloProbabilityDiff = game.HomeEloProbability - game.AwayEloProbability, 
            
            HomeGlickoRating = game.HomeGlickoRating,
            AwayGlickoRating = game.AwayGlickoRating,
            GlickoRatingDiff = game.HomeGlickoRating - game.AwayGlickoRating,
            
            HomeGlickoRatingDeviation = game.HomeGlickoRatingDeviation,
            AwayGlickoRatingDeviation = game.AwayGlickoRatingDeviation,
            GlickoRatingDeviationDiff = game.HomeGlickoRatingDeviation - game.AwayGlickoRatingDeviation,
            
            HomeGlickoVolatility = game.HomeGlickoVolatility,
            AwayGlickoVolatility = game.AwayGlickoVolatility,
            GlickoVolatilityDiff = game.HomeGlickoVolatility - game.AwayGlickoVolatility,
            
            HomeGlickoProbability = game.HomeGlickoProbability,
            AwayGlickoProbability = game.AwayGlickoProbability,
            GlickoProbabilityDiff = game.HomeGlickoProbability - game.AwayGlickoProbability, 
            
            HomeOdds = game.HomeOdds ?? float.NaN,
            AwayOdds = game.AwayOdds ?? float.NaN,
            OddsDiff = game.HomeOdds == null ? float.NaN : game.HomeOdds!.Value - game.AwayOdds!.Value,
            
            HomeTotalWinPercentage = game.HomeTotalWinPercentage,
            AwayTotalWinPercentage = game.AwayTotalWinPercentage,
            TotalWinPercentageDiff = game.HomeTotalWinPercentage - game.AwayTotalWinPercentage,
            
            HomeWinPercentageAtHome = game.HomeWinPercentageAtHome,
            AwayWinPercentageWhenAway = game.AwayWinPercentageWhenAway,
            
            HomeLastTenGamesWinPercentage = game.HomeLastTenGamesWinPercentage,
            AwayLastTenGamesWinPercentage = game.AwayLastTenGamesWinPercentage,
            LastTenGamesWinPercentageDiff = game.HomeLastTenGamesWinPercentage - game.AwayLastTenGamesWinPercentage, 
            
            HomeOffensiveRating = game.HomeOffensiveRating,
            AwayOffensiveRating = game.AwayOffensiveRating,
            
            HomeDefensiveRating = game.HomeDefensiveRating,
            AwayDefensiveRating = game.AwayDefensiveRating,
            
            OffensiveRatingDiff = game.HomeOffensiveRating - game.AwayOffensiveRating,
            DefensiveRatingDiff = game.HomeDefensiveRating - game.AwayDefensiveRating,
            
            HomeCurrentStreak = game.HomeCurrentStreak,
            AwayCurrentStreak = game.AwayCurrentStreak,
            CurrentStreakDiff = game.HomeCurrentStreak - game.AwayCurrentStreak,
            
            HomeRestDays = game.HomeRestDaysBeforeGame,
            AwayRestDays = game.AwayRestDaysBeforeGame,
            RestDaysDiff = game.HomeRestDaysBeforeGame - game.AwayRestDaysBeforeGame,
            
            HomeBackToBack = game.HomeBackToBack ? 1 : 0,
            AwayBackToBack = game.AwayBackToBack ? 1 : 0
        });
    }

    private IEstimator<ITransformer> BuildPipeline()
    {
        var transforms = new List<IEstimator<ITransformer>>();
        var teamEncodingFeatures = new List<string>();

        AddCategoryTransform(_classifierConfig.HomeTeamConfig, "HomeTeamEncoded", nameof(NbaGameFeatures.HomeTeamIdentifier));
        AddCategoryTransform(_classifierConfig.AwayTeamConfig, "AwayTeamEncoded", nameof(NbaGameFeatures.AwayTeamIdentifier));
        AddCategoryTransform(_classifierConfig.HomeTeamConfig, "MatchupEncoded", nameof(NbaGameFeatures.MatchupIdentifier));

        var combinedFeatures = _features.Concat(teamEncodingFeatures).ToArray();
        transforms.Add(_mlContext.Transforms.Concatenate("Features", combinedFeatures));
    
        var trainer = CreateTrainer(_mlContext, _options);
        transforms.Add(trainer);
    
        return ChainEstimators(transforms);

        void AddCategoryTransform(CategoryConfig? encodingConfig, string outputName, string featureName)
        {
            if (encodingConfig is null)
                return;
        
            switch (encodingConfig.EncodingType)
            {
                case EncodingType.OneHotEncoding:
                    transforms.Add(_mlContext.Transforms.Categorical.OneHotEncoding(outputName, featureName));
                    break;
                case EncodingType.OneHotHashEncoding:
                    transforms.Add(_mlContext.Transforms.Categorical.OneHotHashEncoding(outputName, featureName, numberOfBits: encodingConfig.Bits));
                    break;
                default:
                    throw new ArgumentException("Unsupported encoding type");
            } 
        
            teamEncodingFeatures.Add(outputName);
        }
    }

    private static IEstimator<ITransformer> ChainEstimators(List<IEstimator<ITransformer>> estimators)
    {
        var pipeline = estimators[0];
        
        for (var i = 1; i < estimators.Count; i++)
            pipeline = pipeline.Append(estimators[i]);
        
        return pipeline;
    }
}