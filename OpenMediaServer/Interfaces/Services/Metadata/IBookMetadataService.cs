using System;
using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IBookMetadataService : ITableBaseService<MetadataBookModel>
{
    Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId);
}
