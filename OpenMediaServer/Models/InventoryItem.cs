using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Models;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public virtual string Category { get; set; }
    public Guid? MetadataId { get; set; }
    public IEnumerable<InventoryItemVersion>? Versions { get; set; }
    public IEnumerable<InventoryItemAddon>? Addons { get; set; }

    /// <summary>
    /// Folder path. Only set if item is in a folder other than the category folder
    /// </summary>
    public string? FolderPath { get; set; }
}
