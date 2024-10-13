namespace OpenMediaServer.Models;

public class Show : InventoryItem
{
    public override string Category => "Show";
    public IEnumerable<Guid>? SeasonIds { get; set; }
}
