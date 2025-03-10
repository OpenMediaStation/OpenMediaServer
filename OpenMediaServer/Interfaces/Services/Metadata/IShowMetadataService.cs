using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IShowMetadataService
{
    Task<MetadataModel> GetShowMetadata(string? year, string title, string? language, Guid metadataId);
    Task<MetadataModel> GetSeasonMetadata(string? year, string title, string? language, Guid metadataId, int? season);
    Task<MetadataModel> GetEpisodeMetadata(string? year, string title, string? language, Guid metadataId, int? season, int? episode);
}
