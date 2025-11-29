using System.Collections.Generic;

namespace NbaOracle.Predictions;

public class TrainingDataSet
{
    public List<NbaGameTrainingData> TrainingGames { get; } = [];
    
    public void AddGame(GameInfo gameInfo, NbaHistoricalModel model)
    {
        var trainingData = NbaGameTrainingDataFactory.Create(gameInfo, model);
        TrainingGames.Add(trainingData);
    }
}