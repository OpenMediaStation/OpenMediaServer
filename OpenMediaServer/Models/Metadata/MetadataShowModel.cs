using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

public class MetadataShowModel
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Year { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Rated { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Released { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Runtime { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Genre { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Director { get; set; }
    [Column(TypeName = "TEXT")]
    public string? Writer { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Actors { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Plot { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Language { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Country { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Awards { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Poster { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? PosterBlurHash { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Backdrop { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? BackdropBlurHash { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Logo { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? LogoBlurHash { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Metascore { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? ImdbRating { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? ImdbVotes { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? ImdbID { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Type { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? DVD { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? BoxOffice { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Production { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Website { get; set; }
}
