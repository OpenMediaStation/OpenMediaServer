using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using SubtitleStream = OpenMediaServer.Models.FileInfo.SubtitleStream;

namespace OpenMediaServer.Services.FileInfo;
 
public class SubtitleStreamStreamService(IDataRepository dataRepository) : TableBaseService<SubtitleStream>(dataRepository), ISubtitleStreamService
{
    public async Task<Guid?> CreateSubtitleStream(IMediaAnalysis mappingInput, Guid mediaDataId)
    {
        var streams = mappingInput.SubtitleStreams.Select(i => MapSubtitleStream(i, mediaDataId)).ToList();

        foreach (var stream in streams)
        {
            await UpdateOrInsert(stream);
        }
        
        var primaryStream = streams.FirstOrDefault(i => i.Index == mappingInput.PrimarySubtitleStream?.Index);
        
        return primaryStream?.Id;
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