using System;
using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.Predictions.Elo;
using NbaOracle.Predictions.Glicko;
using Newtonsoft.Json.Linq;

namespace NbaOracle.Predictions;

public record GameInfo(Game Game, GameBettingOdds? Odds);

public class NbaHistoricalModel
{
    public IEloCalculator EloCalculator { get; }
    public IGlickoCalculator GlickoCalculator { get; }
    private readonly Dictionary<string, TeamStatistics> _teams;
    
    public NbaHistoricalModel(IEloCalculator eloCalculator, IGlickoCalculator glickoCalculator, HashSet<string> teamIdentifiers)
    {
        EloCalculator = eloCalculator;
        GlickoCalculator = glickoCalculator;
        
        _teams = new Dictionary<string, TeamStatistics>();

        foreach (var teamIdentifier in teamIdentifiers)
        {
            var team = new TeamStatistics(teamIdentifier);
            _teams[teamIdentifier] = team;
        }
    }

    public NbaHistoricalModel Copy()
    {
        var teamIdentifiers = _teams.Keys.ToHashSet();
        var model = new NbaHistoricalModel(EloCalculator, GlickoCalculator, teamIdentifiers);
        
        foreach (var team in model._teams)
        {
            team.Value.CopyFrom(_teams[team.Key]);
        }

        return model;
    }

    public TeamStatistics GetTeam(string teamIdentifier)
    {
        return _teams[teamIdentifier];
    }
    
    public void Evolve(GameInfo gameInfo)
    {
        var game = gameInfo.Game;
        
        var home = _teams[game.HomeTeam];
        var away = _teams[game.AwayTeam];
        
        var (homeEloRating, awayEloRating) = EloCalculator.Calculate(home, away, game);
        
        var homeGlickoRating = GlickoCalculator.CalculateRating(home.GlickoScore, away.GlickoScore, game); 
        var awayGlickoRating = GlickoCalculator.CalculateRating(away.GlickoScore, home.GlickoScore, game);
        
        home.AddGame(game, homeEloRating, homeGlickoRating);
        away.AddGame(game, awayEloRating, awayGlickoRating);
    }
    
    public void Regress(double eloRatingMean = 1500.0, double regressionFactor = 0.75) 
    {
        foreach (var team in _teams.Values)
            team.Regress(eloRatingMean, regressionFactor);
    }
}

public class TeamStatistics
{
    public TeamStatistics(string teamIdentifier)
    {
        TeamIdentifier = teamIdentifier;
        
        EloRating = 1500;
        EloRatings = [new EloRating(DateOnly.MinValue, EloRating)];
        GlickoScore = new GlickoScore(teamIdentifier);
        FourFactors = new FourFactorsScore();
    }

    public void Regress(double eloRatingMean, double regressionFactor)
    {
        EloRating = eloRatingMean + regressionFactor * (EloRating - eloRatingMean); // move to specifc classy  todo
        
        const double glickoRatingMean = 1500.0;
        GlickoScore.Rating = glickoRatingMean + regressionFactor * (GlickoScore.Rating - glickoRatingMean);
        GlickoScore.RatingDeviation = Math.Min(350.0, GlickoScore.RatingDeviation + 50.0);
        
        LastGameDate = null;
        FourFactors.Regress();
        
        TotalGames = 0;
        TotalWinPercentage = 0;
        TotalWinPercentage = 0;
        Streak = 0;

        HomeGames = 0;
        HomeWins = 0;
        HomeWinPercentage = 0;

        AwayGames = 0;
        AwayWins = 0;
        AwayWinPercentage = 0;
        
        LastTenGamesOutcome.Clear();
        LastTenGameWinPercentage = 0;
        
        LastTenGamesOffensiveRating.Clear();
        LastTenGamesOffensiveRatingPercentage = 0;
        
        LastTenGamesDefensiveRating.Clear();
        LastTenGamesDefensiveRatingPercentage = 0;

        RestDays = 0;
    }

    public string TeamIdentifier { get; set; }
    
    public DateOnly? LastGameDate { get; set; }
    
    public double EloRating { get; set; }
    public double EloMomentum5Games { get; set; }
    public double EloMomentum10Games { get; set; }
    public List<EloRating> EloRatings { get; }
    
    public GlickoScore GlickoScore { get; }
    
    public FourFactorsScore FourFactors { get; } 
    
    public int Streak { get; set; }
    
    public int TotalGames { get; set; }
    public int TotalWins { get; set; }
    public double TotalWinPercentage { get; private set; }
    
    public int HomeGames { get; set; }
    public int HomeWins { get; set; }
    public double HomeWinPercentage { get; private set; }
    
    public int AwayWins { get; set; }
    public int AwayGames { get; set; }
    public double AwayWinPercentage { get; private set; }

    public List<int> LastTenGamesOutcome { get; private set; } = [];
    public double LastTenGameWinPercentage { get; private set; }

    public List<int> LastTenGamesOffensiveRating { get; private set; } = [];
    public double LastTenGamesOffensiveRatingPercentage { get; private set; }
    
    public List<int> LastTenGamesDefensiveRating { get; private set; } = [];
    public double LastTenGamesDefensiveRatingPercentage { get; private set; }
    
    public int RestDays { get; private set; }
    
    public void AddGame(Game game, double eloRating, GlickoRating glickoRating)
    {
        SetRestDays(game);
        LastGameDate = game.GameDate;

        SetCurrentStreak(game);
        SetEloRating(game, eloRating);
        
        SetGlickoRating(game, glickoRating);

        SetHomeWins(game);
        SetAwayWins(game);

        SetLastTenGameWinPercentage(game);

        SetLastTenGameOffensiveRating(game);
        SetLastTenGameDefensiveRating(game);
        
        FourFactors.Apply(game.HomeTeam == TeamIdentifier ? game.HomeBoxScore.FourFactors : game.AwayBoxScore.FourFactors);
    }

    public void CopyFrom(TeamStatistics teamStatistics)
    {
        throw new NotSupportedException("fix");
        // todo maintain this
        LastGameDate = teamStatistics.LastGameDate;
        
        EloRating = teamStatistics.EloRating;
        EloRatings.Add(teamStatistics.EloRatings.OrderByDescending(x => x.Date).First());

        Streak = teamStatistics.Streak;

        TotalGames = teamStatistics.TotalGames;
        TotalWins = teamStatistics.TotalWins;
        TotalWinPercentage = teamStatistics.TotalWinPercentage;

        HomeGames = teamStatistics.HomeGames;
        HomeWins = teamStatistics.HomeWins;
        HomeWinPercentage = teamStatistics.HomeWinPercentage;
        
        AwayWins = teamStatistics.AwayWins;
        AwayGames = teamStatistics.AwayGames;
        AwayWinPercentage = teamStatistics.AwayWinPercentage;
        
        LastTenGamesOutcome = teamStatistics.LastTenGamesOutcome.Select(x => x).ToList();
        LastTenGameWinPercentage = teamStatistics.LastTenGameWinPercentage;
        
        LastTenGamesOffensiveRating = teamStatistics.LastTenGamesOffensiveRating.Select(x => x).ToList();
        LastTenGamesOffensiveRatingPercentage = teamStatistics.LastTenGamesOffensiveRatingPercentage;
        
        LastTenGamesDefensiveRating = teamStatistics.LastTenGamesDefensiveRating.Select(x => x).ToList();
        LastTenGamesDefensiveRatingPercentage = teamStatistics.LastTenGamesDefensiveRatingPercentage;

        RestDays = teamStatistics.RestDays;
    }
    
    private void SetEloRating(Game game, double eloRating)
    {
        EloRating = eloRating;
        
        EloRatings.Add(new EloRating(game.GameDate, eloRating));
        if (EloRatings.Count > 20)
            EloRatings.RemoveAt(0);

        var last5 = EloRatings.OrderByDescending(x => x.Date).Skip(5).FirstOrDefault();
        EloMomentum5Games = EloRating - last5?.Rating ?? 0.0;
        
        var last10 = EloRatings.OrderByDescending(x => x.Date).Skip(10).FirstOrDefault();
        EloMomentum10Games = EloRating - last10?.Rating ?? 0.0;
    }
    
    private void SetGlickoRating(Game game, GlickoRating glicko)
    {
        GlickoScore.Rating = glicko.Rating;
        GlickoScore.RatingDeviation = glicko.RatingDeviation;
        GlickoScore.Volatility = glicko.Volatility;
        GlickoScore.LastGameDate = game.GameDate;
    }
    
    private void SetCurrentStreak(Game game)
    {
        Streak = TeamIdentifier == game.WinTeam 
            ? Math.Max(1, Streak + 1) 
            : Math.Min(-1, Streak - 1);
    }

    private void SetHomeWins(Game game)
    {
        if (TeamIdentifier != game.HomeTeam)
            return;

        TotalGames++;
        HomeGames++;

        if (TeamIdentifier == game.WinTeam)
        {
            TotalWins++;
            HomeWins++;
        }

        TotalWinPercentage = (double) TotalWins / TotalGames;
        HomeWinPercentage = (double) HomeWins / HomeGames;
    }
    
    private void SetAwayWins(Game game)
    {
        if (TeamIdentifier != game.AwayTeam)
            return;
        
        TotalGames++;
        AwayGames++;

        if (TeamIdentifier == game.WinTeam)
        {
            TotalWins++;
            AwayWins++;
        }

        TotalWinPercentage = (double) TotalWins / TotalGames;
        AwayWinPercentage = (double) AwayWins / AwayGames;
    }

    private void SetLastTenGameWinPercentage(Game game)
    {
        var score = TeamIdentifier == game.WinTeam ? 1 : 0;
        LastTenGamesOutcome.Add(score);
        if (LastTenGamesOutcome.Count > 10)
            LastTenGamesOutcome.RemoveAt(0);
        LastTenGameWinPercentage = (double)LastTenGamesOutcome.Sum() / LastTenGamesOutcome.Count;
    }
    
    private void SetLastTenGameOffensiveRating(Game game)
    {
        var score = TeamIdentifier == game.HomeTeam ? game.HomePoints : game.AwayPoints;
        LastTenGamesOffensiveRating.Add(score);
        if (LastTenGamesOffensiveRating.Count > 10)
            LastTenGamesOffensiveRating.RemoveAt(0);
        LastTenGamesOffensiveRatingPercentage = LastTenGamesOffensiveRating.Average();
    }
    
    private void SetLastTenGameDefensiveRating(Game game)
    {
        var score = TeamIdentifier == game.HomeTeam ? game.AwayPoints : game.HomePoints;
        LastTenGamesDefensiveRating.Add(score);
        if (LastTenGamesDefensiveRating.Count > 10)
            LastTenGamesDefensiveRating.RemoveAt(0);
        LastTenGamesDefensiveRatingPercentage = LastTenGamesDefensiveRating.Average();
    }

    private void SetRestDays(Game game)
    {
        RestDays = GetRestDays(game.GameDate);
    }
    
    public int GetRestDays(DateOnly date)
    {
        if (LastGameDate == null)
            return 0;

        var restDays = date.DayNumber - LastGameDate!.Value.DayNumber;
        
        return restDays > 10 ? 0 : restDays;
    }
}

public record EloRating(DateOnly Date, double Rating);

public record FourFactorsScore
{
    private const int WindowSize = 10;
    
    public void Apply(FourFactors fourFactors)
    {
        AddAndTrim(Pace, fourFactors.Pace);
        AddAndTrim(Efg, fourFactors.Efg);
        AddAndTrim(Tov, fourFactors.Tov);
        AddAndTrim(Orb, fourFactors.Orb);
        AddAndTrim(Ftfga, fourFactors.Ftfga);
        AddAndTrim(Ortg, fourFactors.Ortg);
        
        AvgPace = Pace.Average();
        AvgEfg = Efg.Average();
        AvgTov = Tov.Average();
        AvgOrb = Orb.Average();
        AvgFtfga = Ftfga.Average();
        AvgOrtg = Ortg.Average();
    }

    public void Regress()
    {
        Pace.Clear();
        Efg.Clear();
        Tov.Clear();
        Orb.Clear();
        Ftfga.Clear();
        Ortg.Clear();

        AvgPace = 0;
        AvgEfg = 0;
        AvgTov = 0;
        AvgOrb = 0;
        AvgFtfga = 0;
        AvgOrtg = 0;
    }
    
    private static void AddAndTrim(List<decimal> list, decimal value)
    {
        list.Add(value);
        if (list.Count > WindowSize)
        {
            list.RemoveAt(0);
        }
    }

    private List<decimal> Pace { get; set; } = [];
    public decimal AvgPace { get; private set; }
    
    private List<decimal> Efg { get; set; } = [];
    public decimal AvgEfg { get; private set; }
    
    private List<decimal> Tov { get; set; } = [];
    public decimal AvgTov { get; private set; }
    
    private List<decimal> Orb { get; set; } = [];
    public decimal AvgOrb { get; private set; }
    
    private List<decimal> Ftfga { get; set; } = [];
    public decimal AvgFtfga { get; private set; }
    
    private List<decimal> Ortg { get; set; } = [];
    public decimal AvgOrtg { get; private set; }
}