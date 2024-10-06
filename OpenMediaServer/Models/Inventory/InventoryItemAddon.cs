using System;

namespace OpenMediaServer.Models.Inventory;

/// <summary>
/// Something like .nfo files, subtitles, covers...
/// </summary>
public class InventoryItemAddon
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public string Category { get; set; }
}
