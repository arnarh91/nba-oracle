using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace NbaOracle.Predictions.Classifiers;

public class AveragedPerceptronClassifier : ClassifierBase<AveragedPerceptronTrainer.Options>
{
    public AveragedPerceptronClassifier(AveragedPerceptronTrainer.Options options, ClassifierConfig classifierConfig, string[] features) : base(options, classifierConfig, features) { }
    
    protected override IEstimator<ITransformer> CreateTrainer(MLContext mlContext, AveragedPerceptronTrainer.Options options)
        => mlContext.BinaryClassification.Trainers.AveragedPerceptron(options);
    
    public static AveragedPerceptronTrainer.Options DefaultOptions { get; } = new();
}