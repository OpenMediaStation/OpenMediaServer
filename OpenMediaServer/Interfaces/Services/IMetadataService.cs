using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services;

public interface IMetadataService
{
    Task<MetadataModel?> CreateNewMetadata(string category, Guid parentId, string title, string? year = null);
    Task<IEnumerable<MetadataModel>> ListMetadata(string category);
    Task<MetadataModel?> GetMetadata(string category, Guid id);
    Task<bool> UpdateOrAddMetadata(MetadataModel metadataModel);
}
