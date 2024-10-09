using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IStreamingService
{
    Task<Stream?> GetMediaStream(Guid id, string category, Guid? versionId = null);
    Task<IResult> GetTranscodingPlaylist(Guid id, string category, HttpRequest request, HttpResponse response, Guid? versionId = null);
    Task<IResult> GetTranscodingSegment(Guid id, string category, HttpContext context, double segmentStart, double segmentEnd, Guid? versionId = null);
}
