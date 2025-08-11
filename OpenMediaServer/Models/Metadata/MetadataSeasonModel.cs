using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

public class MetadataSeasonModel
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Poster { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? PosterBlurHash { get; set; }
    
    [Column(TypeName = "DATE")]
    public DateTime? AirDate { get; set; }
    
    [Column(TypeName = "TEXT")]
    public int? EpisodeCount { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Overview { get; set; }
}

