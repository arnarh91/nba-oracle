using System;
using NbaOracle.Data.Games;

namespace NbaOracle.Predictions.Elo;

public interface IEloCalculator
{
    (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game);
}

public record RestDayConfiguration(
    double BackToBack = -50,
    double OneDay = 0,
    double TwoDays = 25,
    double ThreePlusDays = 15
);

public class StandardEloCalculator(double k) : IEloCalculator
{
    public (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var expectedHome = ExpectedScore(home.EloRating, away.EloRating);
        var expectedAway = 1.0 - expectedHome;

        var updatedHomeEloRating = home.EloRating + k * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + k * (1 - score - expectedAway);

        return (updatedHomeEloRating, updatedAwayEloRating);
    }
    
    private static double ExpectedScore(double ratingA, double ratingB)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));
    }
}

public class HomeAdvantageEloCalculator(double k, double homeAdvantage) : IEloCalculator
{
    public (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var expectedHome = 1.0 / (1.0 + Math.Pow(10.0, ((away.EloRating - (home.EloRating + homeAdvantage)) / 400.0)));
        var expectedAway = 1.0 - expectedHome;

        var updatedHomeEloRating = home.EloRating + k * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + k * (1 - score - expectedAway);

        return (updatedHomeEloRating, updatedAwayEloRating);
    }
}

public class HomeAdvantageWithMarginEloCalculator(double k, double homeAdvantage) : IEloCalculator
{
    public (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var margin = game.HomePoints - game.AwayPoints;
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var adjustedHome = home.EloRating + homeAdvantage;
        var expectedHome = 1.0 / (1.0 + Math.Pow(10.0, (away.EloRating - adjustedHome) / 400.0));
        
        var multiplier = Math.Log(Math.Abs(margin) + 1) * (2.2 / ((expectedHome - 0.5) * 2.2 + 2.2));
        
        var updatedHomeEloRating = home.EloRating + k * multiplier * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + k * multiplier * ((1 - score) - (1 - expectedHome));
        
        return (updatedHomeEloRating, updatedAwayEloRating);
    }
}

public class MarginWithRestDaysEloCalculator(double k, double homeAdvantage, RestDayConfiguration restDayConfiguration) : IEloCalculator
{
    public (double updatedHomeEloRating, double updatedAwayEloRating) Calculate(TeamStatistics home, TeamStatistics away, Game game)
    {
        var homeRestDays = home.GetRestDays(game.GameDate);
        var awayRestDays = away.GetRestDays(game.GameDate);
        
        var score = game.HomePoints > game.AwayPoints ? 1 : 0;
        
        var adjustedHome = home.EloRating + homeAdvantage + GetRestAdjustment(homeRestDays);
        var adjustedAway = away.EloRating + GetRestAdjustment(awayRestDays);
        
        var expectedHome = 1.0 / (1.0 + Math.Pow(10.0, (adjustedAway - adjustedHome) / 400.0));

        var multiplier = Math.Log(Math.Abs(game.HomePoints - game.AwayPoints) + 1) * (2.2 / ((expectedHome - 0.5) * 2.2 + 2.2));

        
        var updatedHomeEloRating = home.EloRating + k * multiplier * (score - expectedHome);
        var updatedAwayEloRating = away.EloRating + k * multiplier * ((1 - score) - (1 - expectedHome));

        return (updatedHomeEloRating, updatedAwayEloRating);
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