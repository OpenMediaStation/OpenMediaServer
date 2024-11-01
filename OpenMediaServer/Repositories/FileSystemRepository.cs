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

        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public async Task<T?> ReadObject<T>(string path)
    {
        try
        {
            var text = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(text, options: Globals.JsonOptions);
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

    public IEnumerable<string> EnumerateFiles(string path)
    {
        try
        {
            var files = Directory.EnumerateFiles(path);
            return files;
        }
        catch (DirectoryNotFoundException dirEx)
        {
            _logger.LogDebug(dirEx, "Directory could not be found while enumerating files");

            return [];
        }
    }

    public IEnumerable<string> GetFiles(string path, string searchPattern)
    {
        try
        {
            var files =  Directory.GetFiles(path, searchPattern);
            return files;
        }
        catch (DirectoryNotFoundException dirEx)
        {
            _logger.LogDebug(dirEx, "Directory could not be found while getting files");

            return [];
        }
    }

    public async Task WriteBytes(string path, byte[] bytes)
    {
        FileInfo file = new FileInfo(path);
        file.Directory?.Create();

        await File.WriteAllBytesAsync(path, bytes);
    }

    public Stream? GetStream(string path)
    {
        try
        {
            var stream = new FileStream(path, FileMode.Open);
            return stream;
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogWarning(fileEx, "Stream could not be found");
            return null;
        }
    }
}
