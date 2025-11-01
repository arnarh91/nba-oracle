namespace NbaOracle.Predictions.Classifiers;

public class ClassifierConfig
{
    public int Seed { get; set; } = 42;
    
    public CategoryConfig? HomeTeamConfig { get; set; }
    public CategoryConfig? AwayTeamConfig { get; set; }
    public CategoryConfig? MatchupConfig { get; set; }
    
    public static ClassifierConfig None => new();
    
    public static ClassifierConfig Matchup(CategoryConfig matchupConfig) => new() 
    { 
        MatchupConfig = matchupConfig 
    };
    
    public static ClassifierConfig All(CategoryConfig homeConfig, CategoryConfig awayConfig, CategoryConfig matchupConfig) => new() 
    { 
        HomeTeamConfig = homeConfig, 
        AwayTeamConfig = awayConfig, 
        MatchupConfig = matchupConfig 
    };
}

public enum EncodingType
{
    OneHotEncoding,
    OneHotHashEncoding
}
public record CategoryConfig
{
    public EncodingType EncodingType { get; set; }
    public int Bits { get; set; }
    
    public static CategoryConfig OneHot(int bits = 8) => new()
    {
        EncodingType = EncodingType.OneHotEncoding,
        Bits = bits
    };
    
    public static CategoryConfig OneHotHash(int bits = 10) => new()
    {
        EncodingType = EncodingType.OneHotHashEncoding,
        Bits = bits
    };
}