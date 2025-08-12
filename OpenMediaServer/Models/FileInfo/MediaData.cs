using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FFMpegCore;

namespace OpenMediaServer.Models.FileInfo;

public class MediaData
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "INTERVAL")]
    public TimeSpan Duration { get; set; }
    
    [ForeignKey(nameof(MediaFormat)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid MediaFormatId { get; set; }
    
    [ForeignKey(nameof(AudioStream)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid PrimaryAudioStreamId { get; set; }    
    
    [ForeignKey(nameof(VideoStream)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid PrimaryVideoStreamId { get; set; }  
    
    [ForeignKey(nameof(SubtitleStream)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid PrimarySubtitleStreamId { get; set; }
}
