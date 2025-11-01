using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace NbaOracle.Predictions.Classifiers;

public class SdcaLogisticRegressionClassifier : ClassifierBase<SdcaLogisticRegressionBinaryTrainer.Options>
{
    public SdcaLogisticRegressionClassifier(SdcaLogisticRegressionBinaryTrainer.Options options, ClassifierConfig classifierConfig, string[] features) : base(options, classifierConfig, features) { }
    
    protected override IEstimator<ITransformer> CreateTrainer(MLContext mlContext, SdcaLogisticRegressionBinaryTrainer.Options options)
    {
        return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(options);
    }
    
    public static SdcaLogisticRegressionBinaryTrainer.Options DefaultOptions { get; } = new();
}