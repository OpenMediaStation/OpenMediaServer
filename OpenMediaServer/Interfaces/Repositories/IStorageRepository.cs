using System;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IStorageRepository
{
    public Task WriteText(string path, string text);
    public Task<string> ReadText(string path);
    public Task WriteObject<T>(string path, T item);
    public Task<T> ReadObject<T>(string path);
}
