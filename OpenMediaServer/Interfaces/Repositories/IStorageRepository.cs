using System;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IStorageRepository
{
    public Task WriteText(string path, string text);
    public Task<string> ReadText(string path);
}
