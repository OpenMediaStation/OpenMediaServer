using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class MediaStream
{
    [Column(TypeName = "INTEGER")]
    public int Index { get; set; }

    [Column(TypeName = "TEXT")]
    public string CodecName { get; set; }

    [Column(TypeName = "TEXT")]
    public string CodecLongName { get; set; }

    [Column(TypeName = "TEXT")]
    public string CodecTagString { get; set; }

    [Column(TypeName = "TEXT")]
    public string CodecTag { get; set; }

    [Column(TypeName = "BIGINT")]
    public long BitRate { get; set; }

    [NotMapped]
    public TimeSpan StartTime { get; set; }

    [NotMapped]
    public TimeSpan Duration { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Language { get; set; }

    [NotMapped]
    public Dictionary<string, bool>? Disposition { get; set; }

    [NotMapped]
    public Dictionary<string, string>? Tags { get; set; }

    [Column(TypeName = "INTEGER")]
    public int? BitDepth { get; set; }
}
