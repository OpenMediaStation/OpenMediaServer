namespace OpenMediaServer.DTOs.Endpoints.FileInfo;

public class AudioStreamDto : MediaStreamDto
{
    public int Channels { get; set; }

    public string ChannelLayout { get; set; }

    public int SampleRateHz { get; set; }

    public string Profile { get; set; }
}