using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models;

public class Show : InventoryItem
{
    [Column(TypeName = "TEXT")]
    public override string Category => "Show";
    public IEnumerable<Guid>? SeasonIds { get; set; }
}
