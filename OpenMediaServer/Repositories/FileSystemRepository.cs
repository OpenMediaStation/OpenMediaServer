using System;
using System.Text.Json;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Repositories;

public class FileSystemRepository : IStorageRepository
{
    private readonly ILogger<FileSystemRepository> _logger;

    public FileSystemRepository(ILogger<FileSystemRepository> logger)
    {
        _logger = logger;
    }

    public async Task WriteText(string path, string text)
    {
        await File.WriteAllTextAsync(path, text);
    }

    public async Task<string> ReadText(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteObject<T>(string path, T item)
    {
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(item));
    }

    public async Task<T> ReadObject<T>(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(text);
    }

    // public async Task WriteData()
    // {

    // }

    // public async string ReadData()
    // {

    // }
}
