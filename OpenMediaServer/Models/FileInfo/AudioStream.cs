using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class AudioStream : MediaStream
{
    [Column(TypeName = "INTEGER")]
    public int Channels { get; set; }

    [Column(TypeName = "TEXT")]
    public string? ChannelLayout { get; set; }

    [Column(TypeName = "INTEGER")]
    public int SampleRateHz { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Profile { get; set; }
}
