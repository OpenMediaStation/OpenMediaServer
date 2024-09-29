using System;

namespace OpenMediaServer.Models;

public class Episode : InventoryItem
{
    public override string Category => "Episode";
    public Guid SeasonId { get; set; }
    public int EpisodeNr { get; set; }
}