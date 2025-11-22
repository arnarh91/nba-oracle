using System;
using System.Collections.Generic;

namespace NbaOracle.Tests.Integration.Predictions;

public class PredictionPerformanceTracker
{
    public PredictionPerformanceTracker(string instanceName)
    {
        InstanceName = instanceName;
    }

    public string InstanceName { get; }
    public int GamesCount { get; private set; }
    public int CorrectPredictionCount { get; private set; }
    public List<GamePredictionResult> GamePredictions { get; } = [];
    public decimal PredictionAccuracy => Math.Round((decimal)CorrectPredictionCount / GamesCount, 4);

    public void AddPrediction(GamePredictionResult gamePrediction)
    {
        GamePredictions.Add(gamePrediction);
        GamesCount++;

        if (gamePrediction.PredictedWinTeam == gamePrediction.WinTeam)
            CorrectPredictionCount++;
    }
}

public record GamePredictionResult(int GameId, string WinTeam, string PredictedWinTeam, double Confidence)
{
    public override string ToString()
    {
        if (WinTeam == PredictedWinTeam)
            return $"CORRECT - {PredictedWinTeam} - {Confidence}";
        
        return $"WRONG - {WinTeam} won {PredictedWinTeam} - {Confidence}";
    }
}