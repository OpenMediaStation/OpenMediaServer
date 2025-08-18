namespace OpenMediaServer.DTOs.Endpoints.Inventory;

public class InventoryItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public virtual string Category { get; set; }
    public Guid? MetadataId { get; set; }
    public IEnumerable<InventoryItemVersionDto>? Versions { get; set; }
    public IEnumerable<AddonDto>? Addons { get; set; }
    public string? DisplayImageBlurHash { get; set; }

    /// <summary>
    /// Folder path. Only set if item is in a folder other than the category folder
    /// </summary>
    public string? FolderPath { get; set; }
}