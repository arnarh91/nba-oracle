using Microsoft.ML;
using Microsoft.ML.Trainers.LightGbm;
// ReSharper disable ConvertToPrimaryConstructor

namespace NbaOracle.Predictions.Classifiers;

public class LightGbmClassifier : ClassifierBase<LightGbmBinaryTrainer.Options>
{
    public LightGbmClassifier(LightGbmBinaryTrainer.Options options, ClassifierConfig config, string[] features) : base(options, config, features) { }

    protected override IEstimator<ITransformer> CreateTrainer(MLContext mlContext, LightGbmBinaryTrainer.Options options)
        => mlContext.BinaryClassification.Trainers.LightGbm(options); 

    public static LightGbmBinaryTrainer.Options DefaultOptions { get; } = new()
    {
        NumberOfIterations = 200,
        LearningRate = 0.1f,
        NumberOfLeaves = 31,
        MinimumExampleCountPerLeaf = 20,
        UseCategoricalSplit = false,
        HandleMissingValue = true,
        MinimumExampleCountPerGroup = 100,
        MaximumBinCountPerFeature = 255,
        Booster = new GradientBooster.Options
        {
            L1Regularization = 0.01,
            L2Regularization = 0.01,
            MaximumTreeDepth = 6
        },
        LabelColumnName = "Label",
        FeatureColumnName = "Features"
    };
}