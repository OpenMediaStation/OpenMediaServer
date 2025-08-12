using FFMpegCore;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IMediaDataService   : ITableBaseService<MediaData>
{
    Task<MediaData> CreateMediaData(IMediaAnalysis mappingInput);
}