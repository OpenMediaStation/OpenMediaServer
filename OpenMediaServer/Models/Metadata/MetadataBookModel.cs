using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

public class MetadataBookModel
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Author { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? Publisher { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? PublishedDate { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
        
    [Column(TypeName = "TEXT")]
    public int? PageCount { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? Language { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? Thumbnail { get; set; }
        
    [Column(TypeName = "TEXT")]
    public string? ThumbnailBlurHash { get; set; }
}
