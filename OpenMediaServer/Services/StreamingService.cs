using System;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class StreamingService : IStreamingService
{
    private readonly ILogger<StreamingService> _logger;

    public StreamingService(ILogger<StreamingService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> GetMediaStream(string id, string category)
    {
        // TODO Implement

        throw new NotImplementedException();
    }
}
