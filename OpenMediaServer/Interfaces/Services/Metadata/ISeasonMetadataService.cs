using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface ISeasonMetadataService : ITableBaseService<MetadataSeasonModel>
{
    Task<MetadataModel> GetSeasonMetadata(string? year, string title, string? language, Guid metadataId, int? season);
}