using FFMpegCore;
using MediaFormat = OpenMediaServer.Models.FileInfo.MediaFormat;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IMediaFormatService   : ITableBaseService<MediaFormat>
{
    Task<MediaFormat> CreateMediaFormat(IMediaAnalysis mappingInput);
}