using FFMpegCore;
using VideoStream = OpenMediaServer.Models.FileInfo.VideoStream;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IVideoStreamService  : ITableBaseService<VideoStream>
{
    Task<VideoStream?> CreateVideoStream(IMediaAnalysis mappingInput, Guid mediaDataId);
}