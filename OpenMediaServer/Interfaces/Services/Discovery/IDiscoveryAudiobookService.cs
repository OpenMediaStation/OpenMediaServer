using System;

namespace OpenMediaServer.Interfaces.Services.Discovery;

public interface IDiscoveryAudiobookService
{
    Task CreateAudiobook(string path);
}
