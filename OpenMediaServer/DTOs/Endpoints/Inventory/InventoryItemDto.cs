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
    
    // Show
    public IEnumerable<Guid>? SeasonIds { get; set; }
    
    public IEnumerable<Guid>? EpisodeIds { get; set; }
    public Guid? ShowId { get; set; }
    public int? SeasonNr { get; set; } 
    public Guid? SeasonId { get; set; }
    public int? EpisodeNr { get; set; }
}