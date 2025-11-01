using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.Trainers.FastTree;
using NbaOracle.Data.GameBettingOdds;
using NbaOracle.Data.Games;
using NbaOracle.Predictions.Classifiers;
using NbaOracle.Predictions.Elo;

namespace NbaOracle.Predictions;

public record GameInfo(Game Game, GameBettingOdds? Odds);

public class NbaHistoricalModel
{
    private readonly IEloCalculator _eloCalculator; 
    private readonly Dictionary<string, TeamStatistics> _teams;
    
    public NbaHistoricalModel(IEloCalculator eloCalculator, HashSet<string> teamIdentifiers)
    {
        _eloCalculator = eloCalculator;
        
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
        var model = new NbaHistoricalModel(_eloCalculator, teamIdentifiers);
        
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
    
    public EloRating GetPreviousRating(string teamIdentifier, DateOnly gameDate)
    {
        var team = _teams[teamIdentifier];
        var scoreByDate = team.EloRatings.Where(x => x.Date < gameDate).OrderByDescending(x => x.Date).First();
        return scoreByDate;
    }

    public void Evolve(GameInfo gameInfo)
    {
        var game = gameInfo.Game;
        
        var home = _teams[game.HomeTeam];
        var away = _teams[game.AwayTeam];
        
        var (updatedHomeEloRating, updatedAwayEloRating) = _eloCalculator.Calculate(home, away, game);
        
        home.AddGame(game, updatedHomeEloRating);
        away.AddGame(game, updatedAwayEloRating);
    }
    
    public void Regress(double mean = 1500.0, double regressionFactor = 0.75) 
    {
        foreach (var team in _teams.Values)
        {
            team.EloRating = mean + regressionFactor * (team.EloRating - mean);
            team.Streak = 0;
            team.LastGameDate = null;
        }
    }
}

public class TeamStatistics
{
    public TeamStatistics(string teamIdentifier)
    {
        TeamIdentifier = teamIdentifier;
        EloRating = 1500;
        EloRatings = [new EloRating(DateOnly.MinValue, EloRating)];
    }

    public string TeamIdentifier { get; set; }
    
    public DateOnly? LastGameDate { get; set; }
    
    public double EloRating { get; set; }
    public double EloMomentum5Games { get; set; }
    public double EloMomentum10Games { get; set; }
    public List<EloRating> EloRatings { get; }
    
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
    
    public void AddGame(Game game, double eloRating)
    {
        SetRestDays(game);
        LastGameDate = game.GameDate;

        SetCurrentStreak(game);
        SetEloRating(game, eloRating);

        SetHomeWins(game);
        SetAwayWins(game);

        SetLastTenGameWinPercentage(game);

        SetLastTenGameOffensiveRating(game);
        SetLastTenGameDefensiveRating(game);
    }

    public void CopyFrom(TeamStatistics teamStatistics)
    {
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