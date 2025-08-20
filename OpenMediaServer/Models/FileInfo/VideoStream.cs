using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class VideoStream
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
    
    [Column(TypeName = "TEXT")] 
    public string Category { get; set; } = "VideoStream";
    
    [Column(TypeName = "DOUBLE PRECISION")]
    public double AvgFrameRate { get; set; }

    [Column(TypeName = "INTEGER")]
    public int BitsPerRawSample { get; set; }

    [Column(TypeName = "TEXT")]
    public string Profile { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Width { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Height { get; set; }

    [Column(TypeName = "DOUBLE PRECISION")]
    public double FrameRate { get; set; }

    [Column(TypeName = "TEXT")]
    public string PixelFormat { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Rotation { get; set; }

    [Column(TypeName = "DOUBLE PRECISION")]
    public double AverageFrameRate { get; set; }
}
