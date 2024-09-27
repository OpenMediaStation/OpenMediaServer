using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IStreamingService
{
    public Task<Stream> GetMediaStream(string id, string category);
}
