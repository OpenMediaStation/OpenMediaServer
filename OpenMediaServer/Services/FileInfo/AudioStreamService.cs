using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using AudioStream = OpenMediaServer.Models.FileInfo.AudioStream;

namespace OpenMediaServer.Services.FileInfo;

public class AudioStreamService(IDataRepository dataRepository)
    : TableBaseService<AudioStream>(dataRepository), IAudioStreamService
{
    public async Task<Guid?> CreateAudioStreams(IMediaAnalysis mappingInput, Guid mediaDataId)
    {
        var streams = mappingInput.AudioStreams.Select(i => MapAudioStream(i, mediaDataId)).ToList();

        foreach (var stream in streams)
        {
            await UpdateOrInsert(stream);
        }
        
        var primaryStream = streams.FirstOrDefault(i => i.Index == mappingInput.PrimaryAudioStream?.Index);
        
        return primaryStream?.Id;    
    }

    private AudioStream MapAudioStream(FFMpegCore.AudioStream input, Guid mediaDataId)
    {
        return new AudioStream()
        {
            Id = Guid.NewGuid(),
            MediaDataId = mediaDataId,

            Channels = input.Channels,
            ChannelLayout = input.ChannelLayout,
            SampleRateHz = input.SampleRateHz,
            Profile = input.Profile,

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