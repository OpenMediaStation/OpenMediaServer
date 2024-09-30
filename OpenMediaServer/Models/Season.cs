using System;

namespace OpenMediaServer.Models;

public class Season : InventoryItem
{
    public override string Category => "Season";
    public Guid ShowId { get; set; }
    public int SeasonNr { get; set; }
}
