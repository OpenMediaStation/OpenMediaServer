using System;
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

    // public async Task WriteData()
    // {

    // }

    // public async string ReadData()
    // {

    // }
}
