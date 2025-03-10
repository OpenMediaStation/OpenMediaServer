using System;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IBookMetadataService
{
    Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId);
}
