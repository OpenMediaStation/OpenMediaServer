using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IContentDiscoveryService
{
    void Watch(string path);
    Task ActiveScan(string path);
    Task MoveToBinIfDeleted();
}
