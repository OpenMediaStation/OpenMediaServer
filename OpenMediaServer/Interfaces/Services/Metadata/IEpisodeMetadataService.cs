using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IEpisodeMetadataService: ITableBaseService<MetadataEpisodeModel>
{
    Task<MetadataModel> GetEpisodeMetadata(string? year, string title, string? language, Guid metadataId, int? season, int? episode);
}