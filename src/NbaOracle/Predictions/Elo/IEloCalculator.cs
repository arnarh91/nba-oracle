using System;
using NbaOracle.Data.Games;

namespace NbaOracle.Predictions.Elo;

public interface IEloCalculator
{
    (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game);
    double PredictWinProbability(TeamStatistics home, TeamStatistics away, Game game);
}

public record RestDayConfiguration(
    double BackToBack = -50,
    double OneDay = 0,
    double TwoDays = 25,
    double ThreePlusDays = 15
);

public abstract class EloCalculatorBase(double k) : IEloCalculator
{
    protected double K { get; } = k;
    
    public abstract (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game);
    
    public abstract double PredictWinProbability(TeamStatistics home, TeamStatistics away, Game game);
    
    protected static double CalculateExpectedScore(double ratingA, double ratingB)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));
    }
}

public class StandardEloCalculator(double k) : EloCalculatorBase(k)
{
    public override (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var expectedHome = CalculateExpectedScore(home.EloRating, away.EloRating);
        var expectedAway = 1.0 - expectedHome;

        var updatedHomeEloRating = home.EloRating + K * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + K * (1 - score - expectedAway);

        return (updatedHomeEloRating, updatedAwayEloRating);
    }
    
    public override double PredictWinProbability(TeamStatistics homeTeam, TeamStatistics awayTeam, Game game)
    {
        return CalculateExpectedScore(homeTeam.EloRating, awayTeam.EloRating);
    }
}

public class HomeAdvantageEloCalculator(double k, double homeAdvantage) : EloCalculatorBase(k)
{
    public override (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var adjustedHome = home.EloRating + homeAdvantage;
        var expectedHome = CalculateExpectedScore(adjustedHome, away.EloRating);
        var expectedAway = 1.0 - expectedHome;

        var updatedHomeEloRating = home.EloRating + K * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + K * (1 - score - expectedAway);

        return (updatedHomeEloRating, updatedAwayEloRating);
    }
    
    public override double PredictWinProbability(TeamStatistics homeTeam, TeamStatistics awayTeam, Game game)
    {
        var adjustedHome = homeTeam.EloRating + homeAdvantage;
        return CalculateExpectedScore(adjustedHome, awayTeam.EloRating);
    }
}

public class HomeAdvantageWithMarginEloCalculator(double k, double homeAdvantage) : EloCalculatorBase(k)
{
    public override (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var margin = game.HomePoints - game.AwayPoints;
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var adjustedHome = home.EloRating + homeAdvantage;
        var expectedHome = CalculateExpectedScore(adjustedHome, away.EloRating);
        
        var multiplier = CalculateMarginMultiplier(margin, expectedHome);
        
        var updatedHomeEloRating = home.EloRating + K * multiplier * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + K * multiplier * ((1 - score) - (1 - expectedHome));
        
        return (updatedHomeEloRating, updatedAwayEloRating);
    }
    
    public override double PredictWinProbability(TeamStatistics homeTeam, TeamStatistics awayTeam, Game game)
    {
        var adjustedHome = homeTeam.EloRating + homeAdvantage;
        return CalculateExpectedScore(adjustedHome, awayTeam.EloRating);
    }

    private static double CalculateMarginMultiplier(int margin, double expectedScore)
    {
        return Math.Log(Math.Abs(margin) + 1) * (2.2 / ((expectedScore - 0.5) * 2.2 + 2.2));
    }
}

public class MarginWithRestDaysEloCalculator(double k, double homeAdvantage, RestDayConfiguration restDayConfiguration) : EloCalculatorBase(k)
{
    public override (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var homeRestDays = home.GetRestDays(game.GameDate);
        var awayRestDays = away.GetRestDays(game.GameDate);
        
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var adjustedHome = home.EloRating + homeAdvantage + GetRestAdjustment(homeRestDays);
        var adjustedAway = away.EloRating + GetRestAdjustment(awayRestDays);
        
        var expectedHome = CalculateExpectedScore(adjustedHome, adjustedAway);

        var multiplier = Math.Log(Math.Abs(game.HomePoints - game.AwayPoints) + 1) * (2.2 / ((expectedHome - 0.5) * 2.2 + 2.2));
        
        var updatedHomeEloRating = home.EloRating + K * multiplier * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + K * multiplier * ((1 - score) - (1 - expectedHome));

        return (updatedHomeEloRating, updatedAwayEloRating);
    }
    
    public override double PredictWinProbability(TeamStatistics home, TeamStatistics away, Game game)
    {
        var homeRestDays = home.GetRestDays(game.GameDate);
        var awayRestDays = away.GetRestDays(game.GameDate);
        
        var adjustedHome = home.EloRating + homeAdvantage + GetRestAdjustment(homeRestDays);
        var adjustedAway = away.EloRating + GetRestAdjustment(awayRestDays);
        
        return CalculateExpectedScore(adjustedHome, adjustedAway);
    }
    
    private double GetRestAdjustment(int restDays)
    {
        if (restDays >= 10)
            return 0;
        
        return restDays switch
        {
            0 => restDayConfiguration.BackToBack,
            1 => restDayConfiguration.OneDay,
            2 => restDayConfiguration.TwoDays,
            >= 3 => restDayConfiguration.ThreePlusDays,
            _ => 0
        };
    }
}