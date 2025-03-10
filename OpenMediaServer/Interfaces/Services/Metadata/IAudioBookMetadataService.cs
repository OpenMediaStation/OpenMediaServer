using System;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IAudioBookMetadataService
{
    Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId);
}
