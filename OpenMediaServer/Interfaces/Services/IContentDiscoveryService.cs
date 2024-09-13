using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IContentDiscoveryService
{
    public void Watch(string path);
    public void ActiveScan(string path);
}
