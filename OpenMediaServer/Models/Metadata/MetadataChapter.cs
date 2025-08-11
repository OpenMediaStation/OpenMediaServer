using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Metadata;

public class MetadataChapter
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(MetadataModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid MetadataModelId { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Title { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public uint? StartTimeInSeconds { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public uint? EndTimeInSeconds { get; set; }
}