using System;
using FFMpegCore;

namespace OpenMediaServer.Models.FileInfo;

public class MediaData
{
    public TimeSpan Duration { get; set; }

    public MediaFormat Format { get; set; }

    public AudioStream? PrimaryAudioStream { get; set; }

    public VideoStream? PrimaryVideoStream { get; set; }

    public SubtitleStream? PrimarySubtitleStream { get; set; }

    public List<VideoStream> VideoStreams { get; set; }

    public List<AudioStream> AudioStreams { get; set; }

    public List<SubtitleStream> SubtitleStreams { get; set; }

    public IReadOnlyList<string> ErrorData { get; set; }
}
