using System;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IFileSystemRepository
{
    Task WriteText(string path, string text);
    Task<string> ReadText(string path);
    IEnumerable<string> EnumerateFiles(string path);
    Task WriteBytes(string path, byte[] bytes);
    Stream? GetStream(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern);
}
