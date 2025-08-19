using OpenMediaServer.DTOs.Endpoints.FileInfo;
using OpenMediaServer.Models.FileInfo;
using AudioStream = OpenMediaServer.Models.FileInfo.AudioStream;
using MediaFormat = OpenMediaServer.Models.FileInfo.MediaFormat;
using SubtitleStream = OpenMediaServer.Models.FileInfo.SubtitleStream;
using VideoStream = OpenMediaServer.Models.FileInfo.VideoStream;

namespace OpenMediaServer.Extensions.Mapping;

public static class FileInfoModelExtensions
{
    public static FileInfoDto ToDto(this FileInfoModel fileInfo, MediaData? mediaData, MediaFormat? mediaFormat, AudioStream? primaryAudioStream, IEnumerable<AudioStream>? audioStreams, SubtitleStream? primarySubtitleStream, IEnumerable<SubtitleStream>? subtitleStreams, VideoStream? primaryVideoStream, IEnumerable<VideoStream>? videoStreams)
    {
        ArgumentNullException.ThrowIfNull(mediaData);
        ArgumentNullException.ThrowIfNull(mediaFormat);

        var result = new FileInfoDto()
        {
            Id = fileInfo.Id,
            ParentCategory = fileInfo.ParentCategory,
            MediaData = new MediaDataDto
            {
                Duration = mediaData.Duration,
                Format = new MediaFormatDto
                {
                    Duration = mediaFormat.Duration,
                    StartTime = mediaFormat.StartTime,
                    FormatName = mediaFormat.FormatName,
                    FormatLongName = mediaFormat.FormatLongName,
                    StreamCount = mediaFormat.StreamCount,
                    ProbeScore = mediaFormat.ProbeScore,
                    BitRate = mediaFormat.BitRate,
                },
                PrimaryAudioStream = MapAudioStream(primaryAudioStream),
                PrimarySubtitleStream = MapSubtitleStream(primarySubtitleStream),
                PrimaryVideoStream = MapVideoStream(primaryVideoStream),
                AudioStreams = audioStreams?.Select(i => i.MapAudioStream()).ToList(),
                SubtitleStreams = subtitleStreams?.Select(i => i.MapSubtitleStream()).ToList(),
                VideoStreams = videoStreams?.Select(i => i.MapVideoStream()).ToList(),
            }
        };
        
        return result;
    }

    public static AudioStreamDto? MapAudioStream(this AudioStream? audioStream)
    {
        if (audioStream == null)
        {
            return null;
        }
        
        return new AudioStreamDto()
        {
            Channels = audioStream.Channels,
            ChannelLayout = audioStream.ChannelLayout,
            SampleRateHz = audioStream.SampleRateHz,
            Profile = audioStream.Profile,
            Index = audioStream.Index,
            CodecName = audioStream.CodecName,
            CodecLongName = audioStream.CodecLongName,
            CodecTagString = audioStream.CodecTagString,
            CodecTag = audioStream.CodecTag,
            BitRate = audioStream.BitRate,
            StartTime = audioStream.StartTime,
            Duration = audioStream.Duration,
            Language = audioStream.Language,
            BitDepth = audioStream.BitDepth,
        };
    }    
    
    public static VideoStreamDto? MapVideoStream(this VideoStream? videoStream)
    {
        if (videoStream == null)
        {
            return null;
        }
        
        return new VideoStreamDto()
        {
            AverageFrameRate = videoStream.AverageFrameRate,
            BitsPerRawSample = videoStream.BitsPerRawSample,
            Profile = videoStream.Profile,
            Width = videoStream.Width,
            Height = videoStream.Height,
            FrameRate = videoStream.FrameRate,
            PixelFormat = videoStream.PixelFormat,
            Rotation = videoStream.Rotation,
            Index = videoStream.Index,
            CodecName = videoStream.CodecName,
            CodecLongName = videoStream.CodecLongName,
            CodecTagString = videoStream.CodecTagString,
            CodecTag = videoStream.CodecTag,
            BitRate = videoStream.BitRate,
            StartTime = videoStream.StartTime,
            Duration = videoStream.Duration,
            Language = videoStream.Language,
            BitDepth = videoStream.BitDepth,
        };
    }    
    
    public static SubtitleStreamDto? MapSubtitleStream(this SubtitleStream? subtitleStream)
    {
        if (subtitleStream == null)
        {
            return null;
        }
        
        return new SubtitleStreamDto()
        {
            Index = subtitleStream.Index,
            CodecName = subtitleStream.CodecName,
            CodecLongName = subtitleStream.CodecLongName,
            CodecTagString = subtitleStream.CodecTagString,
            CodecTag = subtitleStream.CodecTag,
            BitRate = subtitleStream.BitRate,
            StartTime = subtitleStream.StartTime,
            Duration = subtitleStream.Duration,
            Language = subtitleStream.Language,
            BitDepth = subtitleStream.BitDepth,
        };
    }
    
}