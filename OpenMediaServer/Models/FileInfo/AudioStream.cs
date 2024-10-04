using System;

namespace OpenMediaServer.Models.FileInfo;

public class AudioStream : MediaStream
{
    public int Channels { get; set; }

    public string ChannelLayout { get; set; }

    public int SampleRateHz { get; set; }

    public string Profile { get; set; }
}
