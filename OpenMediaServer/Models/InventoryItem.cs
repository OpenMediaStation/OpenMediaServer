using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Models;

public class InventoryItem
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    
    [Column(TypeName = "TEXT")]
    public string? Title { get; set; }
    
    
    [Column(TypeName = "TEXT")]
    public string? Category { get; set; }
    
    
    [ForeignKey(nameof(Metadata.MetadataModel)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? MetadataId { get; set; }
    
    [Column(TypeName = "BOOLEAN")]
    public bool IsOrphan { get; set; }
    
    [Column(TypeName = "DATE")]
    public DateTime? ReleaseDate { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? DisplayImageBlurHash { get; set; }

    /// <summary>
    /// Folder path. Only set if item is in a folder other than the category folder
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? FolderPath { get; set; }
    
    [ForeignKey(nameof(InventoryItem)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? SeasonId { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public int? EpisodeNr { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public int? SeasonNr { get; set; }
    
    [ForeignKey(nameof(InventoryItem)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid? ShowId { get; set; }
}
