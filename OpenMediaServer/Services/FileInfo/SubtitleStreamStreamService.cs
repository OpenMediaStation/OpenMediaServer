using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using SubtitleStream = OpenMediaServer.Models.FileInfo.SubtitleStream;

namespace OpenMediaServer.Services.FileInfo;
 
public class SubtitleStreamStreamService(IDataRepository dataRepository) : TableBaseService<SubtitleStream>(dataRepository), ISubtitleStreamService
{
    public async Task<SubtitleStream?> CreateSubtitleStream(IMediaAnalysis mappingInput, Guid mediaDataId)
    {
        var streams = mappingInput.SubtitleStreams.Select(i => MapSubtitleStream(i, mediaDataId)).ToList();
        
        SubtitleStream? primarySubtitleStream = null;

        if (mappingInput.PrimarySubtitleStream != null)
        {
            primarySubtitleStream = MapSubtitleStream(mappingInput.PrimarySubtitleStream, mediaDataId);
        }

        if (primarySubtitleStream != null)
        {
            streams.Add(primarySubtitleStream);
        }

        foreach (var stream in streams)
        {
            await UpdateOrInsert(stream);
        }
        
        return primarySubtitleStream;
    }
    
    private SubtitleStream MapSubtitleStream(FFMpegCore.SubtitleStream input, Guid mediaDataId)
    {
        return new SubtitleStream()
        {
            Id = Guid.NewGuid(),
            MediaDataId = mediaDataId,
            
            // MediaStream
            Index = input.Index,
            CodecName = input.CodecName,
            CodecLongName = input.CodecLongName,
            CodecTagString = input.CodecTagString,
            CodecTag = input.CodecTag,
            BitRate = input.BitRate,
            StartTime = input.StartTime,
            Duration = input.Duration,
            Language = input.Language,
            BitDepth = input.BitDepth,
        };
    }
}