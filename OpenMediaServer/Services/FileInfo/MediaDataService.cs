using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Services.FileInfo;

public class MediaDataService(IDataRepository dataRepository, IMediaFormatService mediaFormatService, IAudioStreamService audioStreamService, IVideoStreamService videoStreamService, ISubtitleStreamService subtitleStreamService)
    : TableBaseService<MediaData>(dataRepository), IMediaDataService
{
    public async Task<MediaData> CreateMediaData(IMediaAnalysis mappingInput)
    {
        var mediaData = new MediaData
        {
            Id = Guid.NewGuid(),
            Duration = mappingInput.Duration,
        };
        
        var mediaFormat = await mediaFormatService.CreateMediaFormat(mappingInput);
        
        mediaData.MediaFormatId = mediaFormat.Id;
        
        
        await UpdateOrInsert(mediaData);
        
        return mediaData;
    }
}