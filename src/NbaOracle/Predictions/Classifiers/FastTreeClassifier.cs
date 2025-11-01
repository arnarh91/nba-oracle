using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
// ReSharper disable ConvertToPrimaryConstructor

namespace NbaOracle.Predictions.Classifiers;

public class FastTreeClassifier : ClassifierBase<FastTreeBinaryTrainer.Options>
{
    public FastTreeClassifier(FastTreeBinaryTrainer.Options options, ClassifierConfig classifierConfig, string[] features) : base(options, classifierConfig, features) { }

    protected override IEstimator<ITransformer> CreateTrainer(MLContext mlContext, FastTreeBinaryTrainer.Options options)
        => mlContext.BinaryClassification.Trainers.FastTree(options);
    
    public static FastTreeBinaryTrainer.Options DefaultOptions { get; } = new();
}