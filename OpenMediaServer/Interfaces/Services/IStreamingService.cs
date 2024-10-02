using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IStreamingService
{
    Task<Stream?> GetMediaStream(Guid id, string category);
    Task<IResult> GetTranscodedMediaStream(Guid id, string category, HttpRequest request, HttpResponse response);
}
