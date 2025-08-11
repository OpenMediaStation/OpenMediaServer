using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IMovieMetadataService : ITableBaseService<MetadataMovieModel>
{
    Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId);
}
