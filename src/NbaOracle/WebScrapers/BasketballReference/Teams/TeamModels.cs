using System;
using System.Collections.Generic;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.BasketballReference.Teams;

public record TeamData(
    TeamMiscData Misc,
    List<PlayerRosterData> Roster,
    List<PlayerSeasonStatisticsData> PlayerStatistics,
    List<PlayByPlayData> PlayByPlay
);

public record TeamMiscData(int Wins, int WinsLeagueRank, int Losses, int LossesLeagueRank, decimal MarginOfVictory, int MarginOfVictoryLeagueRank, decimal OffensiveRating, decimal OffensiveRatingLeagueRank, decimal DefensiveRating, decimal DefensiveRatingLeagueRank);

public class PlayByPlayData
{
    public static PlayByPlayData Create(string playerName, int games, int minutesPlayed, string plusMinusOnCourt, string plusMinusNetOnOffCourt)
    {
        return new PlayByPlayData(playerName, games, minutesPlayed, string.IsNullOrWhiteSpace(plusMinusOnCourt) ? 0.0m : new PlusMinusScore(plusMinusOnCourt), string.IsNullOrWhiteSpace(plusMinusNetOnOffCourt) ? 0.0m : new PlusMinusScore(plusMinusNetOnOffCourt));
    }

    public string PlayerName { get; }
    public int Games { get; }
    public int MinutesPlayed { get; }
    public decimal PlusMinusOnCourt { get; }
    public decimal PlusMinusNetOnOffCourt { get; }

    public PlayByPlayData(string playerName, int games, int minutesPlayed, decimal plusMinusOnCourt, decimal plusMinusNetOnOffCourt)
    {
        PlayerName = playerName;
        Games = games;
        MinutesPlayed = minutesPlayed;
        PlusMinusOnCourt = plusMinusOnCourt;
        PlusMinusNetOnOffCourt = plusMinusNetOnOffCourt;
    }

    public override string ToString()
    {
        return $"{PlayerName} - ({PlusMinusOnCourt})";
    }
}

public class PlayerSeasonStatisticsData
{
    public string PlayerName { get; }
    public int GamesPlayed { get; }
    public int MinutesPlayed { get; }
    public int FieldGoalsMade { get; }
    public int FieldGoalsAttempted { get; }
    public int ThreePointersMade { get; }
    public int ThreePointersAttempted { get; }
    public int TwoPointersMade { get; }
    public int TwoPointersAttempted { get; }
    public decimal EffectiveFieldGoalPercentage { get; }
    public int FreeThrowsMade { get; }
    public int FreeThrowsAttempted { get; }
    public int OffensiveRebounds { get; }
    public int DefensiveRebounds { get; }
    public int TotalRebounds { get; }
    public int Assists { get; }
    public int Steals { get; }
    public int Blocks { get; }
    public int Turnovers { get; }
    public int PersonalFouls { get; }
    public int Points { get; }

    public PlayerSeasonStatisticsData(string playerName, int gamesPlayed, int minutesPlayed, int fieldGoalsMade, int fieldGoalsAttempted, int threePointersMade, int threePointersAttempted, int twoPointersMade, int twoPointersAttempted, decimal effectiveFieldGoalPercentage, int freeThrowsMade, int freeThrowsAttempted, int offensiveRebounds, int defensiveRebounds, int assists, int  steals, int blocks, int turnovers, int personalFouls, int points)
    {
        PlayerName = playerName;
        
        GamesPlayed = gamesPlayed;
        MinutesPlayed = minutesPlayed;
        
        FieldGoalsMade = fieldGoalsMade;
        FieldGoalsAttempted = fieldGoalsAttempted;

        ThreePointersMade = threePointersMade;
        ThreePointersAttempted = threePointersAttempted;

        TwoPointersMade = twoPointersMade;
        TwoPointersAttempted = twoPointersAttempted;

        EffectiveFieldGoalPercentage = effectiveFieldGoalPercentage;
        
        FreeThrowsMade = freeThrowsMade;
        FreeThrowsAttempted = freeThrowsAttempted;
        
        OffensiveRebounds = offensiveRebounds;
        DefensiveRebounds = defensiveRebounds;
        TotalRebounds = offensiveRebounds + defensiveRebounds;

        Assists = assists;
        Steals = steals;
        Blocks = blocks;
        Turnovers = turnovers;
        PersonalFouls = personalFouls;
        Points = points;
    }

    public override string ToString()
    {
        return $"{PlayerName} - ({Points} points)";
    }
}
    
public class PlayerRosterData
{
    public static PlayerRosterData Create(string name, string jerseyNumber, string position, DateTime birthDate, string birthCountry, string height, string weight, string yearsExperience, string college)
    {
        return new PlayerRosterData(name, jerseyNumber, position, birthDate, birthCountry, new HeightInFeetAndInches(height).ToCm(), new WeightInPounds(weight).ToKiloGrams(), new NumberOfYearInLeague(yearsExperience), new College(college));
    }

    public PlayerRosterData(string name, string jerseyNumber, string position, DateTime birthDate, string birthCountry, decimal height, decimal weight, int numberOfYearInLeague, string college)
    {
        Name = name;
        JerseyNumber = jerseyNumber;
        Position = position;
        BirthDate = birthDate;
        BirthCountry = birthCountry;
        Height = height;
        Weight = weight;
        NumberOfYearInLeague = numberOfYearInLeague;
        College = college;
    }

    public string Name { get; }
    public string JerseyNumber { get; }
    public string Position { get; }
    public DateTime BirthDate { get; }
    public string BirthCountry { get; }
    public decimal Height { get; }
    public decimal Weight { get; }
    public int NumberOfYearInLeague { get; }
    public string College { get; }

    public override string ToString() => $"{Name} ({Position}-{JerseyNumber})";

    public string GetPlayerIdentifier()
    {
        var nameInLowerCase = Name.ToLowerInvariant();
        var nameSplit = nameInLowerCase.Split(" ");
        var nameSplitFormat = string.Join(",", nameSplit);
        return $"{nameSplitFormat}_{BirthDate.Year}-{BirthDate.Month}-{BirthDate.Day}";
    }
}