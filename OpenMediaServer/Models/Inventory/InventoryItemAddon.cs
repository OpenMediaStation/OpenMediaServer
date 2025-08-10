using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Inventory;

/// <summary>
/// Something like .nfo files, subtitles, covers...
/// </summary>
public class InventoryItemAddon
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(InventoryItem)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid InventoryItemId { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string Path { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string Category { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? SubtitleLanguage { get; set; }
}
