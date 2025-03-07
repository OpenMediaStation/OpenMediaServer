using System;

namespace OpenMediaServer.Interfaces.Services.Discovery;

public interface IAudiobookDiscoveryService
{
    Task CreateAudiobook(string path);
}
