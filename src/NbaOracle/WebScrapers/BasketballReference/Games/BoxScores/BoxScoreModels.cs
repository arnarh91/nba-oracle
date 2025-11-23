using System.Collections.Generic;

namespace NbaOracle.WebScrapers.BasketballReference.Games.BoxScores;

public record BoxScoreData(string GameIdentifier, LineScoreData LineScore, TeamBoxScoreData HomeBoxScore, TeamBoxScoreData AwayBoxScore);
public record LineScoreData(string HomeTeam, string AwayTeam, List<QuarterScoreData> Quarters);

public record FourFactorsData(decimal Pace, decimal Efg, decimal Tov, decimal Orb, decimal Ftfga, decimal Ortg);

public record QuarterScoreData(string Quarter, int HomeScore, int AwayScore)
{
    public int GetQuarterNumber()
    {
        if (int.TryParse(Quarter, out var number))
            return number;
        
        return int.Parse(Quarter[0].ToString()) + 4;
    } 
}
public record TeamBoxScoreData
{
    public FourFactorsData FourFactors { get; set; } = null!;
    public List<PlayerBasicBoxScoreData> BasicBoxScore { get; } = [];
    public List<PlayerAdvancedBoxScoreData> AdvancedBoxScore { get; } = [];
    public List<DidNotPlayData> DidNotPlay { get; set; } = [];
}

public record DidNotPlayData(string PlayerName, string Reason);

public record PlayerBasicBoxScoreData(
    string PlayerName,
    bool Starter,
    int SecondsPlayed,
    int FieldGoalsMade,
    int FieldGoalsAttempted,
    int ThreePointersMade,
    int ThreePointersAttempted,
    int FreeThrowsMade,
    int FreeThrowsAttempted,
    int OffensiveRebounds,
    int DefensiveRebounds,
    int Assists,
    int Steals,
    int Blocks,
    int Turnovers,
    int PersonalFouls,
    int Points,
    decimal GameScore,
    int PlusMinusScore
);

public record PlayerAdvancedBoxScoreData(
    string PlayerName,
    decimal TrueShootingPercentage,
    decimal EffectiveFieldGoalPercentage,
    decimal ThreePointAttemptRate,
    decimal FreeThrowsAttemptRate,
    decimal OffensiveReboundPercentage,
    decimal DefensiveReboundPercentage,
    decimal TotalReboundPercentage,
    decimal AssistPercentage,
    decimal StealPercentage,
    decimal BlockPercentage,
    decimal TurnoverPercentage,
    decimal UsagePercentage,
    int OffensiveRating,
    int DefensiveRating,
    decimal BoxPlusMinus
);