using FFMpegCore;
using SubtitleStream = OpenMediaServer.Models.FileInfo.SubtitleStream;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface ISubtitleStreamService  : ITableBaseService<SubtitleStream>
{
    Task<SubtitleStream?> CreateSubtitleStream(IMediaAnalysis mappingInput, Guid mediaDataId);
}