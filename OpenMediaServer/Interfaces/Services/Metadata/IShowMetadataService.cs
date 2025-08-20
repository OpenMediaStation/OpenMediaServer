using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IShowMetadataService : ITableBaseService<MetadataShowModel>
{
    Task<MetadataModel> GetShowMetadata(string? year, string title, string? language, Guid metadataId);
}
