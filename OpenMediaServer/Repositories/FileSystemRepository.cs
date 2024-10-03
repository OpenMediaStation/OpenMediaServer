using System.Text.Json;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Repositories;

public class FileSystemRepository : IFileSystemRepository
{
    private readonly ILogger<FileSystemRepository> _logger;

    public FileSystemRepository(ILogger<FileSystemRepository> logger)
    {
        _logger = logger;
    }

    public async Task WriteText(string path, string text)
    {
        FileInfo file = new FileInfo(path);
        file.Directory?.Create();

        await File.WriteAllTextAsync(path, text);
    }

    public async Task<string> ReadText(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteObject<T>(string path, T item)
    {
        FileInfo file = new FileInfo(path);
        file.Directory?.Create();

        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(item));
    }

    public async Task<T?> ReadObject<T>(string path)
    {
        try
        {
            var text = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(text);
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogDebug(fileEx, "File could not be found");
            return default;
        }
        catch (DirectoryNotFoundException dirEx)
        {
            _logger.LogDebug(dirEx, "Directory could not be found");
            return default;
        }
    }

    public IEnumerable<string> GetFiles(string path)
    {
        try
        {
            var files = Directory.EnumerateFiles(path);
            return files;
        }
        catch (DirectoryNotFoundException dirEx)
        {
            _logger.LogDebug(dirEx, "Directory could not be found while getting files");

            return [];
        }
    }
}
