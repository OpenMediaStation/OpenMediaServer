using System;
using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;
using OpenMediaServer.Services.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IAudioBookMetadataService : ITableBaseService<MetadataAudiobookModel>
{
    Task<MetadataModel> GenerateMetadata(string? year, string title, string? language, Guid metadataId, string? filePath);
}
