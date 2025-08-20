namespace OpenMediaServer.DTOs.Endpoints.FileInfo;

public class MediaDataDto
{
    public TimeSpan Duration { get; set; }

    public MediaFormatDto Format { get; set; }

    public AudioStreamDto? PrimaryAudioStream { get; set; }

    public VideoStreamDto? PrimaryVideoStream { get; set; }

    public SubtitleStreamDto? PrimarySubtitleStream { get; set; }

    public List<VideoStreamDto?>? VideoStreams { get; set; }

    public List<AudioStreamDto?>? AudioStreams { get; set; }

    public List<SubtitleStreamDto?>? SubtitleStreams { get; set; }
}