namespace NbaOracle.WebScrapers.Espn;

public record InjuryOptions
{
    public InjuryOptions(string? filePath)
    {
        _filePath = filePath;
    }

    private readonly string? _filePath;

    public string? GetFilePath() => _filePath;
}