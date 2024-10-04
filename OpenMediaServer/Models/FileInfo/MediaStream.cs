using System;

namespace OpenMediaServer.Models.FileInfo;

public class MediaStream
{
    public int Index { get; set; }

    public string CodecName { get; set; }

    public string CodecLongName { get; set; }

    public string CodecTagString { get; set; }

    public string CodecTag { get; set; }

    public long BitRate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan Duration { get; set; }

    public string? Language { get; set; }

    public Dictionary<string, bool>? Disposition { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public int? BitDepth { get; set; }
}
