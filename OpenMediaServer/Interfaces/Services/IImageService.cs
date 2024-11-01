using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IImageService
{
    Task WriteImage(byte[]? bytes, string url, string fileName, string category, string id);
    Stream? GetImageStream(string? path);
    string? GetPath(string category, Guid metadataId, string type, int? width, int? height);
}
