using System.Collections.Generic;

namespace NbaOracle.Predictions;

public class TrainingDataSet
{
    private const int RollingWindow = 10;
    
    public List<NbaGameTrainingData> TrainingGames { get; } = [];
    
    public void AddGame(GameInfo gameInfo, NbaHistoricalModel model)
    {
        var home = model.GetTeam(gameInfo.Game.HomeTeam);
        var away = model.GetTeam(gameInfo.Game.AwayTeam);

        if (home.TotalGames < RollingWindow || away.TotalGames < RollingWindow)
            return;
        
        var trainingData = NbaGameTrainingDataFactory.Create(gameInfo, model);
        TrainingGames.Add(trainingData);
    }
}