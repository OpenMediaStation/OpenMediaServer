using ATL;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class ChapterService(IDataRepository dataRepository)  : TableBaseService<MetadataChapter>(dataRepository), IChapterService
{
    public async Task ExtractChapters(string? filePath, Guid metadataModelId)
    {
        if (filePath == null)
        {
            return;
        }

        var track = new Track(filePath);
        
        foreach (var item in track.Chapters)
        {
            var chapter = new MetadataChapter()
            {
                Id = Guid.NewGuid(),
                MetadataModelId = metadataModelId,
                Title = item.Title,
                StartTimeInSeconds = item.StartTime / 1000,
                EndTimeInSeconds = item.EndTime / 1000,
            };
            
            await UpdateOrInsert(chapter);
        }
    }
}