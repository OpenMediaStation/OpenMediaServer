using System;
using System.Text.Json;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Repositories;

namespace OpenMediaServer.Test.Mocks;

public class FileSystemRepoMock : IFileSystemRepository
{
    public List<string?> WrittenObjects { get; set; } = new();

    public IEnumerable<string> EnumerateFiles(string path)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetFiles(string path, string searchPattern)
    {
        throw new NotImplementedException();
    }

    public Stream? GetStream(string path)
    {
        throw new NotImplementedException();
    }

    public async Task<T?> ReadObject<T>(string path)
    {
        return default;
    }

    public Task<string> ReadText(string path)
    {
        throw new NotImplementedException();
    }

    public Task WriteBytes(string path, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public async Task WriteObject<T>(string path, T item)
    {
        WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public Task WriteText(string path, string text)
    {
        throw new NotImplementedException();
    }
}
