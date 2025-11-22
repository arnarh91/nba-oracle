using Microsoft.ML.Data;

namespace NbaOracle.Predictions;

public class NbaGameTrainingData
{
    public string HomeIdentifier { get; set; }
    public string AwayIdentifier { get; set; }
    public string MatchupIdentifier { get; set; }
    
    public bool HomeTeamWon { get; set; }
    
    public float HomeEloRating { get; set; }
    public float AwayEloRating { get; set; }
    
    public float HomeEloMomentum5Games { get; set; }
    public float AwayEloMomentum5Games { get; set; }
    
    public float HomeEloMomentum10Games { get; set; }
    public float AwayEloMomentum10Games { get; set; }

    public float HomeEloProbability { get; set; }
    public float AwayEloProbability { get; set; }
    
    public float HomeGlickoRating { get; set; }
    public float AwayGlickoRating { get; set; }
    
    public float HomeGlickoRatingDeviation { get; set; }
    public float AwayGlickoRatingDeviation { get; set; }
    
    public float HomeGlickoVolatility { get; set; }
    public float AwayGlickoVolatility { get; set; }
    
    public float HomeGlickoProbability { get; set; }
    public float AwayGlickoProbability { get; set; }
    
    public float? HomeOdds { get; set; }
    public float? AwayOdds { get; set; }
    
    public float HomeTotalWinPercentage { get; set; }
    public float AwayTotalWinPercentage { get; set; }
    
    public float HomeLastTenGamesWinPercentage { get; set; }
    public float AwayLastTenGamesWinPercentage { get; set; }
    
    public float HomeWinPercentageAtHome { get; set; }
    public float AwayWinPercentageWhenAway { get; set; }
    
    public float HomeOffensiveRating { get; set; }
    public float HomeDefensiveRating { get; set; }
    public float AwayOffensiveRating { get; set; }
    public float AwayDefensiveRating { get; set; }
    
    public float HomeCurrentStreak { get; set; }
    public float AwayCurrentStreak { get; set; }
    
    public float HomeRestDaysBeforeGame { get; set; }
    public float AwayRestDaysBeforeGame { get; set; }
    
    public bool HomeBackToBack { get; set; }
    public bool AwayBackToBack { get; set; }
}

public class NbaGameFeatures
{
    public required string HomeTeamIdentifier { get; set; }
    public required string AwayTeamIdentifier { get; set; }
    public required string MatchupIdentifier { get; set; }
    
    public required float HomeEloRating { get; set; }
    public required float AwayEloRating { get; set; }
    public required float EloDiff { get; set; }
    
    public required float HomeEloMomentum5Games { get; set; }
    public required float AwayEloMomentum5Games { get; set; }
    public required float EloMomentum5GamesDiff { get; set; }
    
    public required float HomeEloMomentum10Games { get; set; }
    public required float AwayEloMomentum10Games { get; set; }
    public required float EloMomentum10GamesDiff { get; set; }
    
    public required float HomeEloProbability { get; set; }
    public required float AwayEloProbability { get; set; }
    public required float EloProbabilityDiff { get; set; }
    
    public required float HomeGlickoRating { get; set; }
    public required float AwayGlickoRating { get; set; }
    public required float GlickoRatingDiff { get; set; }
    
    public required  float HomeGlickoRatingDeviation { get; set; }
    public required float AwayGlickoRatingDeviation { get; set; }
    public required float GlickoRatingDeviationDiff { get; set; }
    
    public required float HomeGlickoVolatility { get; set; }
    public required float AwayGlickoVolatility { get; set; }
    public required float GlickoVolatilityDiff { get; set; }
    
    public required float HomeGlickoProbability { get; set; }
    public required float AwayGlickoProbability { get; set; }
    public required float GlickoProbabilityDiff { get; set; }
    
    public float HomeOdds { get; set; }
    public float AwayOdds { get; set; }
    public float OddsDiff { get; set; }
    
    public required float HomeTotalWinPercentage { get; set; }
    public required float AwayTotalWinPercentage { get; set; }
    public required float TotalWinPercentageDiff { get; set; }
    
    public required float HomeWinPercentageAtHome { get; set; }
    public required float AwayWinPercentageWhenAway { get; set; }
    
    public required float HomeLastTenGamesWinPercentage { get; set; }
    public required float AwayLastTenGamesWinPercentage { get; set; }
    public required float LastTenGamesWinPercentageDiff { get; set; }
    
    public required float HomeOffensiveRating { get; set; }
    public required float AwayOffensiveRating { get; set; }
    public required float OffensiveRatingDiff { get; set; }
    
    public required float HomeDefensiveRating { get; set; }
    public required float AwayDefensiveRating { get; set; }
    public required float DefensiveRatingDiff { get; set; }
    
    public required float HomeCurrentStreak { get; set; }
    public required float AwayCurrentStreak { get; set; }
    public required float CurrentStreakDiff { get; set; }
    
    public required float HomeRestDays { get; set; }
    public required float AwayRestDays { get; set; }
    public required float RestDaysDiff { get; set; }
    
    public required float HomeBackToBack { get; set; }
    public required float AwayBackToBack { get; set; }
    
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