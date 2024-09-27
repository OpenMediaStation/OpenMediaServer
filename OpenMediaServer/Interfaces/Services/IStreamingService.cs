using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IStreamingService
{
    public Task<Stream?> GetMediaStream(Guid id, string category);
}
