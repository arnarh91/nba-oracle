using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
// ReSharper disable ConvertToPrimaryConstructor

namespace NbaOracle.Predictions.Classifiers;

public class FastForestClassifier : ClassifierBase<FastForestBinaryTrainer.Options>
{
    public FastForestClassifier(FastForestBinaryTrainer.Options options, ClassifierConfig classifierConfig, string[] features) : base(options, classifierConfig, features) { }
    
    protected override IEstimator<ITransformer> CreateTrainer(MLContext mlContext, FastForestBinaryTrainer.Options options)
        => mlContext.BinaryClassification.Trainers.FastForest(options).Append(mlContext.BinaryClassification.Calibrators.Platt());
    
    public static FastForestBinaryTrainer.Options DefaultOptions { get; } = new();
}