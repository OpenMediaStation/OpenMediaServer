using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

/// <summary>
/// General model to represent metadata. This should be used for all types of media. Including but not only: Movie, Show, Episode, Season, Audiobook, Book, Song...
/// </summary>
public class MetadataModel
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }

    public string? Title { get; set; }
    [Column(Order=1, TypeName = "TEXT")]
    public string? Category { get; set; }

    // Specific information
    public MetadataMovieModel? Movie { get; set; }
    public MetadataShowModel? Show { get; set; }
    public MetadataSeasonModel? Season { get; set; }
    public MetadataEpisodeModel? Episode { get; set; }
    public MetadataBookModel? Book { get; set; }
    public MetadataAudiobookModel? Audiobook { get; set; }
}