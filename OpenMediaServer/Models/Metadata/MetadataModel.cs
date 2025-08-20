using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

/// <summary>
/// General model to represent metadata. This should be used for all types of media. Including but not only: Movie, Show, Episode, Season, Audiobook, Book, Song...
/// </summary>
public class MetadataModel
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Title { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Category { get; set; }
    

    // Specific information
    [ForeignKey(nameof(MetadataMovieModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? MovieMetadataId { get; set; }    
    
    [ForeignKey(nameof(MetadataShowModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? ShowMetadataId { get; set; }    
    
    [ForeignKey(nameof(MetadataSeasonModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? SeasonMetadataId { get; set; } 
    
    [ForeignKey(nameof(MetadataEpisodeModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? EpisodeMetadataId { get; set; }    
    
    [ForeignKey(nameof(MetadataBookModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? BookMetadataId { get; set; }    
    
    [ForeignKey(nameof(MetadataAudiobookModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? AudiobookMetadataId { get; set; }
}