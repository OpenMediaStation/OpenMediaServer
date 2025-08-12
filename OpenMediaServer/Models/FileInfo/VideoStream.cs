using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class VideoStream : MediaStream
{
    [Column(TypeName = "TEXT")] 
    public string Category { get; set; } = "VideoStream";
    
    [Column(TypeName = "DOUBLE")]
    public double AvgFrameRate { get; set; }

    [Column(TypeName = "INTEGER")]
    public int BitsPerRawSample { get; set; }

    [Column(TypeName = "TEXT")]
    public string Profile { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Width { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Height { get; set; }

    [Column(TypeName = "DOUBLE")]
    public double FrameRate { get; set; }

    [Column(TypeName = "TEXT")]
    public string PixelFormat { get; set; }

    [Column(TypeName = "INTEGER")]
    public int Rotation { get; set; }

    [Column(TypeName = "DOUBLE")]
    public double AverageFrameRate { get; set; }
}
