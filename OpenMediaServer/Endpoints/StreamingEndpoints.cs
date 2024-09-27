using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Endpoints;

public class StreamingEndpoints : IStreamingEndpoints
{
    private readonly ILogger<StreamingEndpoints> _logger;
    private readonly IStreamingService _streamingService;

    public StreamingEndpoints(ILogger<StreamingEndpoints> logger, IStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/stream");

        group.MapGet("/{category}/{id}", StreamContent);
    }

    public async Task<IResult> StreamContent(Guid id, string category)
    {
        var stream = await _streamingService.GetMediaStream(id, category);

        if (stream == null)
        {
            return Results.NotFound("Id not found in category");
        }

        return Results.Stream(stream, enableRangeProcessing: true, contentType: "video/webm"); // TODO content type
    }
}
