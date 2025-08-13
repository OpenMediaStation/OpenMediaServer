using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class MediaFormat
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "INTERVAL")]
    public TimeSpan Duration { get; set; }

    [Column(TypeName = "INTERVAL")]
    public TimeSpan StartTime { get; set; }

    [Column(TypeName = "TEXT")]
    public string FormatName { get; set; }

    [Column(TypeName = "TEXT")]
    public string FormatLongName { get; set; }

    [Column(TypeName = "INTEGER")]
    public int StreamCount { get; set; }

    [Column(TypeName = "DOUBLE PRECISION")]
    public double ProbeScore { get; set; }

    [Column(TypeName = "DOUBLE PRECISION")]
    public double BitRate { get; set; }
}
