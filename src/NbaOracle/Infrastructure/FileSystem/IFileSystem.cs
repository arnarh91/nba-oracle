using System.IO;
using System.Threading.Tasks;

namespace NbaOracle.Infrastructure.FileSystem;

public interface IFileSystem
{
    Task<string?> GetFileContent(string filePath);
    Task SaveFileContent(string filePath, string htmlContent);
}

public class FileSystem : IFileSystem
{
    public async Task<string?> GetFileContent(string filePath)
    {
        if (!File.Exists(filePath))
            return null;
        
        return await File.ReadAllTextAsync(filePath);
    }

    public Task SaveFileContent(string filePath, string htmlContent)
    {
        var directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName!);
        
        return File.WriteAllTextAsync(filePath, htmlContent);
    }
}