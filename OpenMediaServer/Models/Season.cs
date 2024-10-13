using System;

namespace OpenMediaServer.Models;

public class Season : InventoryItem
{
    public override string Category => "Season";
    public IEnumerable<Guid>? EpisodeIds { get; set; }
    public Guid ShowId { get; set; }
    public int? SeasonNr { get; set; }
}
