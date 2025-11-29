using Microsoft.ML.Data;

namespace NbaOracle.Predictions;

public class NbaGameTrainingDataFactory
{
    public static NbaGameTrainingData Create(GameInfo gameInfo, NbaHistoricalModel model)
    {
        var game = gameInfo.Game;
        
        var home = model.GetTeam(game.HomeTeam);
        var away = model.GetTeam(game.AwayTeam);

        var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, gameInfo.Game);
        var awayEloProbability = 1 - homeEloProbability;
        
        var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
        var awayGlickoProbability = 1 - homeGlickoProbability;
        
        return new NbaGameTrainingData
        {
            HomeIdentifier = home.TeamIdentifier,
            AwayIdentifier = away.TeamIdentifier,
            HomeTeamWon = game.WinTeam == game.HomeTeam,
            MatchupIdentifier = game.MatchupIdentifier,
            HomeEloRating = (float) home.EloRating,
            HomeEloMomentum5Games = (float) home.EloMomentum5Games,
            HomeEloMomentum10Games = (float) home.EloMomentum10Games,
            HomeEloProbability = (float) homeEloProbability,
            AwayEloRating = (float)away.EloRating,
            AwayEloMomentum5Games = (float) away.EloMomentum5Games,
            AwayEloMomentum10Games = (float) away.EloMomentum10Games,
            AwayEloProbability = (float) awayEloProbability,
            HomeGlickoRating = (float) home.GlickoScore.Rating,
            AwayGlickoRating = (float) away.GlickoScore.Rating,
            HomeGlickoRatingDeviation = (float) home.GlickoScore.RatingDeviation,
            AwayGlickoRatingDeviation = (float) away.GlickoScore.RatingDeviation,
            HomeGlickoVolatility = (float) home.GlickoScore.Volatility,
            AwayGlickoVolatility = (float) away.GlickoScore.Volatility,
            HomeGlickoProbability = (float) homeGlickoProbability,
            AwayGlickoProbability = (float) awayGlickoProbability,
            HomeOdds = (float?)gameInfo.Odds?.HomeOdds,
            AwayOdds = (float?)gameInfo.Odds?.AwayOdds,
            HomeTotalWinPercentage = (float)home.TotalWinPercentage,
            AwayTotalWinPercentage = (float)away.TotalWinPercentage,
            HomeWinPercentageAtHome = (float)home.HomeWinPercentage,
            AwayWinPercentageWhenAway = (float)away.AwayWinPercentage,
            HomeLastTenGamesWinPercentage = (float)home.LastTenGameWinPercentage,
            AwayLastTenGamesWinPercentage = (float)away.LastTenGameWinPercentage,
            HomeOffensiveRating = (float) home.LastTenGamesOffensiveRatingPercentage,
            AwayOffensiveRating = (float) away.LastTenGamesOffensiveRatingPercentage,
            HomeDefensiveRating = (float) home.LastTenGamesDefensiveRatingPercentage,
            AwayDefensiveRating = (float) away.LastTenGamesDefensiveRatingPercentage,
            HomeCurrentStreak = home.Streak,
            AwayCurrentStreak = away.Streak,
            HomeRestDaysBeforeGame = home.GetRestDays(game.GameDate),
            AwayRestDaysBeforeGame = away.GetRestDays(game.GameDate),
            HomeBackToBack = home.LastGameDate == game.GameDate.AddDays(-1),
            AwayBackToBack = away.LastGameDate == game.GameDate.AddDays(-1),
            
            HomeFourFactor10AvgPace = (float) home.FourFactors.AvgPace,
            HomeFourFactor10AvgEfg = (float) home.FourFactors.AvgEfg,
            HomeFourFactor10AvgTov = (float) home.FourFactors.AvgTov,
            HomeFourFactor10AvgOrb = (float) home.FourFactors.AvgOrb,
            HomeFourFactor10AvgFtfga = (float) home.FourFactors.AvgFtfga,
            HomeFourFactor10AvgOrtg = (float) home.FourFactors.AvgOrtg,
            
            AwayFourFactor10AvgPace = (float) away.FourFactors.AvgPace,
            AwayFourFactor10AvgEfg = (float) away.FourFactors.AvgEfg,
            AwayFourFactor10AvgTov = (float) away.FourFactors.AvgTov,
            AwayFourFactor10AvgOrb = (float) away.FourFactors.AvgOrb,
            AwayFourFactor10AvgFtfga = (float) away.FourFactors.AvgFtfga,
            AwayFourFactor10AvgOrtg = (float) away.FourFactors.AvgOrtg,
            
            HomeGamesLast7Days = home.ScheduleDensity.GamesInLastXDays(game.GameDate),
            AwayGamesLast7Days = away.ScheduleDensity.GamesInLastXDays(game.GameDate)
        };
    }
}

public class NbaGameTrainingData
{
    public required string HomeIdentifier { get; init; }
    public required string AwayIdentifier { get; init; }
    public required string MatchupIdentifier { get; init; }
    
    public required bool HomeTeamWon { get; init; }
    
    public required float HomeEloRating { get; init; }
    public required  float AwayEloRating { get; init; }
    
    public required float HomeEloMomentum5Games { get; init; }
    public required float AwayEloMomentum5Games { get; init; }
    
    public required float HomeEloMomentum10Games { get; init; }
    public required float AwayEloMomentum10Games { get; init; }

    public required float HomeEloProbability { get; init; }
    public required float AwayEloProbability { get; init; }
    
    public required float HomeGlickoRating { get; init; }
    public required float AwayGlickoRating { get; init; }
    
    public required float HomeGlickoRatingDeviation { get; init; }
    public required float AwayGlickoRatingDeviation { get; init; }
    
    public required float HomeGlickoVolatility { get; init; }
    public required float AwayGlickoVolatility { get; init; }
    
    public required float HomeGlickoProbability { get; init; }
    public required float AwayGlickoProbability { get; init; }
    
    public required float? HomeOdds { get; init; }
    public required float? AwayOdds { get; init; }
    
    public required  float HomeTotalWinPercentage { get; init; }
    public required float AwayTotalWinPercentage { get; init; }
    
    public required float HomeLastTenGamesWinPercentage { get; init; }
    public required float AwayLastTenGamesWinPercentage { get; init; }
    
    public required float HomeWinPercentageAtHome { get; init; }
    public required float AwayWinPercentageWhenAway { get; init; }
    
    public required float HomeOffensiveRating { get; init; }
    public required float HomeDefensiveRating { get; init; }
    public required float AwayOffensiveRating { get; init; }
    public required float AwayDefensiveRating { get; init; }
    
    public required float HomeCurrentStreak { get; init; }
    public required float AwayCurrentStreak { get; init; }
    
    public required float HomeRestDaysBeforeGame { get; init; }
    public required float AwayRestDaysBeforeGame { get; init; }
    
    public required bool HomeBackToBack { get; init; }
    public required bool AwayBackToBack { get; init; }

    public required float HomeFourFactor10AvgPace { get; init; }
    public required float HomeFourFactor10AvgEfg { get; init; }
    public required float HomeFourFactor10AvgTov { get; init; }
    public required float HomeFourFactor10AvgOrb { get; init; }
    public required float HomeFourFactor10AvgFtfga { get; init; }
    public required float HomeFourFactor10AvgOrtg { get; init; }
    
    public required float AwayFourFactor10AvgPace { get; init; }
    public required float AwayFourFactor10AvgEfg { get; init; }
    public required float AwayFourFactor10AvgTov { get; init; }
    public required float AwayFourFactor10AvgOrb { get; init; }
    public required float AwayFourFactor10AvgFtfga { get; init; }
    public required float AwayFourFactor10AvgOrtg { get; init; }
    
    public required int HomeGamesLast7Days { get; init; }
    public required int AwayGamesLast7Days { get; init; }
}

public class NbaGameFeatures
{
    public required string HomeTeamIdentifier { get; set; }
    public required string AwayTeamIdentifier { get; set; }
    public required string MatchupIdentifier { get; set; }
    
    public float OddsDiff { get; set; }
    
    public required float EloDiff { get; set; }
    public required float EloMomentum5GamesDiff { get; set; }
    public required float EloMomentum10GamesDiff { get; set; }
    public required float EloProbabilityDiff { get; set; }
    
    public required float GlickoRatingDiff { get; set; }
    public required float GlickoRatingDeviationDiff { get; set; }
    public required float GlickoVolatilityDiff { get; set; }
    public required float GlickoProbabilityDiff { get; set; }
    
    public required float TotalWinPercentageDiff { get; set; }
    public required float HomeWinPercentageAtHome { get; set; }
    public required float AwayWinPercentageWhenAway { get; set; }
    public required float LastTenGamesWinPercentageDiff { get; set; }
    public required float CurrentStreakDiff { get; set; }
    
    public required float OffensiveRatingDiff { get; set; }
    public required float DefensiveRatingDiff { get; set; }
    
    public required float RestDaysDiff { get; set; }
    public required float HomeBackToBack { get; set; }
    public required float AwayBackToBack { get; set; }
    public required float GamesLast7DaysDiff { get; set; }
    
    public required float FourFactor10AvgPaceDiff { get; init; }
    public required float FourFactor10AvgEfgDiff { get; init; }
    public required float FourFactor10AvgTovDiff { get; init; }
    public required float FourFactor10AvgOrbDiff { get; init; }
    public required float FourFactor10AvgFtfgaDiff { get; init; }
    public required float FourFactor10AvgOrtgDiff { get; init; }
    
    [ColumnName("Label")]
    public required bool HomeTeamWon { get; set; }
}

public class NbaGamePrediction
{
    [ColumnName("PredictedLabel")]
    public bool HomeTeamWins { get; set; }
    
    [ColumnName("Probability")] 
    public float Probability { get; set; }
    
    [ColumnName("Score")]
    public float Score { get; set; }
}