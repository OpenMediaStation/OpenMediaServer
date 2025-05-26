using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Models;

public class InventoryItem
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid Id { get; set; }
    
    
    [Column(Order = 3 ,TypeName = "TEXT")]
    public string? Title { get; set; }
    
    
    [Column(Order = 1, TypeName = "TEXT")]
    public virtual string Category { get; set; }
    
    
    [ForeignKey(nameof(Metadata.MetadataModel)+"(Id)")]
    [Column(Order = 2, TypeName = "UUID")]
    public Guid? MetadataId { get; set; }
    
    [Column(Order = 4, TypeName = "BOOLEAN")]
    public bool IsOrphan { get; set; }
    
    public DateOnly? Year { get; set; }
    
    public IEnumerable<InventoryItemVersion>? Versions { get; set; }
    public IEnumerable<InventoryItemAddon>? Addons { get; set; }
    public string? DisplayImageBlurHash { get; set; }

    /// <summary>
    /// Folder path. Only set if item is in a folder other than the category folder
    /// </summary>
    [Column(Order = 5, TypeName = "TEXT")]
    public string? FolderPath { get; set; }
}
