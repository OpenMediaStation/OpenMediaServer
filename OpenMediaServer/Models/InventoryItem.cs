using System;

namespace OpenMediaServer.Models;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string Path { get; set; }
    public virtual string Category { get; set; }
    public Guid? MetadataId { get; set; }
}
