using FFMpegCore;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using VideoStream = OpenMediaServer.Models.FileInfo.VideoStream;

namespace OpenMediaServer.Services.FileInfo;

public class VideoStreamService(IDataRepository dataRepository) : TableBaseService<VideoStream>(dataRepository), IVideoStreamService
{
    public async Task<VideoStream?> CreateVideoStream(IMediaAnalysis mappingInput, Guid mediaDataId)
    {
        var streams = mappingInput.VideoStreams.Select(i => MapVideoStream(i, mediaDataId)).ToList();
        
        VideoStream? primaryVideoStream = null;

        if (mappingInput.PrimaryVideoStream != null)
        {
            primaryVideoStream = MapVideoStream(mappingInput.PrimaryVideoStream, mediaDataId);
        }

        if (primaryVideoStream != null)
        {
            streams.Add(primaryVideoStream);
        }

        foreach (var stream in streams)
        {
            await UpdateOrInsert(stream);
        }
        
        return primaryVideoStream;
    }
    
    private VideoStream MapVideoStream(FFMpegCore.VideoStream input, Guid mediaDataId)
    {
        return new VideoStream()
        {
            Id = Guid.NewGuid(),
            MediaDataId = mediaDataId,
            
            AvgFrameRate = input.AvgFrameRate,
            BitsPerRawSample = input.BitsPerRawSample,
            Profile = input.Profile,
            Width = input.Width,
            Height = input.Height,
            FrameRate = input.FrameRate,
            PixelFormat = input.PixelFormat,
            Rotation = input.Rotation,
            AverageFrameRate = input.AverageFrameRate,

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