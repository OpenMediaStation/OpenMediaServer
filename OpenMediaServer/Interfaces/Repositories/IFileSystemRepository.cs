using System;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IFileSystemRepository
{
    Task WriteText(string path, string text);
    Task<string> ReadText(string path);
    //TODO ___________REMOVE___________
    // Task WriteObjectAsync<T>(string path, T item);
    // Task<T?> ReadObject<T>(string path);
    //______________REMOVE_END__________
    IEnumerable<string> EnumerateFiles(string path);
    Task WriteBytes(string path, byte[] bytes);
    Stream? GetStream(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern);
}
