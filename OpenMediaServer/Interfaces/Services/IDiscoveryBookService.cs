using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IDiscoveryBookService
{
    Task CreateBook(string path);
}
