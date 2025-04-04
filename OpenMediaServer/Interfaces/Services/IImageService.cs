using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IImageService
{
    Task<string?> WriteImage(byte[]? bytes, string url, string fileName, string category, string id, string? imageType = null);
    Stream? GetImageStream(string? path);
    string? GetPath(string category, Guid metadataId, string type, int? width, int? height);
    string? CreateBlurHash(byte[]? bytes);
}
