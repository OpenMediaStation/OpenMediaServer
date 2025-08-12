using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class MediaStream
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(MediaData)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid MediaDataId { get; set; }
    
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

    [Column(TypeName = "INTERVAL")]
    public TimeSpan StartTime { get; set; }

    [Column(TypeName = "INTERVAL")]
    public TimeSpan Duration { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Language { get; set; }

    [Column(TypeName = "INTEGER")]
    public int? BitDepth { get; set; }
}
