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
        var group = app.MapGroup("/stream").RequireAuthorization();

        group.MapGet("/{category}/{id}", StreamContent);
        group.MapGet("/{category}/{id}/segments/segment{segmentStart}-{segmentEnd}.ts", StreamSegment);
    }

    public async Task<IResult> StreamSegment(HttpContext context, double segmentStart, double segmentEnd, string category, Guid id, Guid? versionId = null)
    {
        _logger.LogTrace("Streaming segment");

        return await _streamingService.GetTranscodingSegment(id, category, context, segmentStart, segmentEnd, versionId);
    }

    public async Task<IResult> StreamContent(Guid id, string category, HttpRequest request, HttpResponse response, bool transcode = false, Guid? versionId = null)
    {
        if (transcode)
        {
            return await _streamingService.GetTranscodingPlaylist(id, category, request, response);
        }
        else
        {
            var stream = await _streamingService.GetMediaStream(id, category, versionId);

            if (stream == null)
            {
                return Results.NotFound("Id not found in category");
            }

            var mimeType = await _streamingService.GetMimeType(id, category, versionId);

            return Results.Stream(stream, enableRangeProcessing: true, contentType: mimeType);
        }
    }
}
