using System;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IFileSystemRepository
{
    Task WriteText(string path, string text);
    Task<string> ReadText(string path);
    Task WriteObject<T>(string path, T item);
    Task<T?> ReadObject<T>(string path);
    IEnumerable<string> GetFiles(string path);
    Task WriteBytes(string path, byte[] bytes);
    Stream? GetStream(string path);
}
