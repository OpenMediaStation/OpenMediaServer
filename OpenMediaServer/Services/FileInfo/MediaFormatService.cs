using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using MediaFormat = OpenMediaServer.Models.FileInfo.MediaFormat;

namespace OpenMediaServer.Services.FileInfo;

public class MediaFormatService(IDataRepository dataRepository) : TableBaseService<MediaFormat>(dataRepository), IMediaFormatService
{
    public async Task<MediaFormat> CreateMediaFormat(IMediaAnalysis mappingInput)
    {
        var format = new MediaFormat()
        {
            Id = Guid.NewGuid(),
            Duration = mappingInput.Duration,
            StartTime = mappingInput.Format.StartTime,
            FormatName = mappingInput.Format.FormatName,
            FormatLongName = mappingInput.Format.FormatLongName,
            StreamCount = mappingInput.Format.StreamCount,
            ProbeScore = mappingInput.Format.ProbeScore,
            BitRate = mappingInput.Format.BitRate,
        };
        
        await UpdateOrInsert(format);
        
        return format;
    }
}