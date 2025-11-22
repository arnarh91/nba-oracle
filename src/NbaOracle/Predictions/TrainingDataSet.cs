using System.Collections.Generic;

namespace NbaOracle.Predictions;

public class TrainingDataSet
{
    public List<NbaGameTrainingData> TrainingGames { get; } = [];
    
    public void AddGame(GameInfo gameInfo, NbaHistoricalModel model)
    {
        var game = gameInfo.Game;
        
        var home = model.GetTeam(game.HomeTeam);
        var away = model.GetTeam(game.AwayTeam);

        var homeEloProbability = model.EloCalculator.PredictWinProbability(home, away, gameInfo.Game);
        var awayEloProbability = 1 - homeEloProbability;
        
        var homeGlickoProbability = model.GlickoCalculator.PredictWinProbability(home.GlickoScore, away.GlickoScore);
        var awayGlickoProbability = 1 - homeGlickoProbability;
        
        var trainingData = new NbaGameTrainingData
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
        };
        
        TrainingGames.Add(trainingData);
    }
}