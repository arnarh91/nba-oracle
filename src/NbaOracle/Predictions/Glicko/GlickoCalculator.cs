using System;
using NbaOracle.Data.Games;

namespace NbaOracle.Predictions.Glicko;

public class GlickoScore
{
    public string TeamIdentifier { get; set; } 
    public double Rating { get; set; }
    public double RatingDeviation { get; set; }
    public double Volatility { get; set; } = 0.06;
    public DateOnly? LastGameDate { get; set; }

    public GlickoScore(string teamIdentifier, double rating = 1500, double rd = 350)
    {
        TeamIdentifier = teamIdentifier;
        Rating = rating;
        RatingDeviation = rd;
    }
}

// todo better naming

public record GlickoRating(double Rating, double RatingDeviation, double Volatility);

public interface IGlickoCalculator
{
    GlickoRating CalculateRating(GlickoScore team, GlickoScore opponent, Game game);
    double PredictWinProbability(GlickoScore team, GlickoScore opponent);
}

public class Glicko2Calculator : IGlickoCalculator
{
    private const double Tau = 0.5;
    private const double Epsilon = 0.000001;
    private const double DefaultRd = 350.0;
    private const double MinRd = 30.0;

    public GlickoRating CalculateRating(GlickoScore team, GlickoScore opponent, Game game)
    {
        // Update RD for time decay
        UpdateRatingDeviationForInactivity(team, game.GameDate);
        UpdateRatingDeviationForInactivity(opponent, game.GameDate);

        // Step 1: Convert ratings to Glicko-2 scale
        var mu = (team.Rating - 1500.0) / 173.7178;
        var phi = team.RatingDeviation / 173.7178;
        var sigma = team.Volatility;

        var opponentMu = (opponent.Rating - 1500.0) / 173.7178;
        var opponentPhi = opponent.RatingDeviation / 173.7178;

        // Step 2: Compute v (variance)
        var g = G(opponentPhi);
        var e = E(mu, opponentMu, opponentPhi);
        var v = 1.0 / (g * g * e * (1.0 - e));

        // Step 3: Compute delta (performance difference)
        var score = game.WinTeam == team.TeamIdentifier ? 1.0 : 0.0;
        var delta = v * g * (score - e);

        // Step 4: Update volatility (σ)
        var newSigma = UpdateVolatility(sigma, phi, v, delta);

        // Step 5: Update rating deviation (φ*)
        var phiStar = Math.Sqrt(phi * phi + newSigma * newSigma);

        // Step 6: Update rating deviation (φ')
        var newPhi = 1.0 / Math.Sqrt((1.0 / (phiStar * phiStar)) + (1.0 / v));

        // Step 7: Update rating (μ')
        var newMu = mu + newPhi * newPhi * g * (score - e);

        // Step 8: Convert back to original scale
        var rating = 173.7178 * newMu + 1500.0;
        var ratingDeviation = 173.7178 * newPhi;
        ratingDeviation = Math.Max(ratingDeviation, MinRd);

        return new GlickoRating(rating, ratingDeviation, newSigma);
    }

    private double UpdateVolatility(double sigma, double phi, double v, double delta)
    {
        var a = Math.Log(sigma * sigma);
        var deltaSq = delta * delta;
        var phiSq = phi * phi;
        const double tauSq = Tau * Tau;

        // Define f(x)
        double F(double x)
        {
            var ex = Math.Exp(x);
            var num1 = ex * (deltaSq - phiSq - v - ex);
            var denom1 = 2.0 * Math.Pow(phiSq + v + ex, 2);
            var num2 = x - a;
            var denom2 = tauSq;
            return (num1 / denom1) - (num2 / denom2);
        }

        // Initialize iterative algorithm
        var A = a;
        double B;

        if (deltaSq > phiSq + v)
        {
            B = Math.Log(deltaSq - phiSq - v);
        }
        else
        {
            var k = 1.0;
            while (F(a - k * Tau) < 0)
            {
                k++;
            }

            B = a - k * Tau;
        }

        var fA = F(A);
        var fB = F(B);

        // Illinois algorithm for finding root
        while (Math.Abs(B - A) > Epsilon)
        {
            var C = A + (A - B) * fA / (fB - fA);
            var fC = F(C);

            if (fC * fB <= 0)
            {
                A = B;
                fA = fB;
            }
            else
            {
                fA = fA / 2.0;
            }

            B = C;
            fB = fC;
        }

        return Math.Exp(A / 2.0);
    }

    private void UpdateRatingDeviationForInactivity(GlickoScore team, DateOnly currentGameDate)
    {
        if (team.LastGameDate == null)
        {
            team.LastGameDate = currentGameDate;
            return;
        }

        var daysSinceLastGame = (currentGameDate.ToDateTime(TimeOnly.MinValue) - team.LastGameDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays;

        // For NBA, consider each week as a rating period
        // Adjust this based on your needs (daily, per game, etc.)
        var ratingPeriods = daysSinceLastGame / 7.0;

        if (ratingPeriods > 0)
        {
            // Pre-rating period step: increase RD based on volatility
            var phi = team.RatingDeviation / 173.7178;
            var sigma = team.Volatility;

            var newPhi = Math.Sqrt(phi * phi + ratingPeriods * sigma * sigma);
            team.RatingDeviation = Math.Min(173.7178 * newPhi, DefaultRd);
        }
    }

    private static double G(double phi)
    {
        return 1.0 / Math.Sqrt(1.0 + 3.0 * phi * phi / (Math.PI * Math.PI));
    }

    private static double E(double mu, double muJ, double phiJ)
    {
        return 1.0 / (1.0 + Math.Exp(-G(phiJ) * (mu - muJ)));
    }

    public double PredictWinProbability(GlickoScore team, GlickoScore opponent)
    {
        var mu = (team.Rating - 1500.0) / 173.7178;
        var opponentMu = (opponent.Rating - 1500.0) / 173.7178;

        // Try using individual team RD instead of combined
        var teamPhi = team.RatingDeviation / 173.7178;
        var opponentPhi = opponent.RatingDeviation / 173.7178;
    
        // Use the average or max RD for uncertainty
        var effectivePhi = Math.Max(teamPhi, opponentPhi);
    
        var g = G(effectivePhi);
        return 1.0 / (1.0 + Math.Exp(-g * (mu - opponentMu)));
    }
}