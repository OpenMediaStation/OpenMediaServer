using OpenMediaServer.Helpers;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Interfaces.Services.Metadata;

public interface IChapterService : ITableBaseService<MetadataChapter>
{
    Task ExtractChapters(string? filePath, Guid metadataModelId);
}