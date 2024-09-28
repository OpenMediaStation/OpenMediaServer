using System;

namespace OpenMediaServer.Models;

public class Movie : InventoryItem
{
    public override string Category => "Movie";
    public MovieShowMetadataModel? Metadata { get; set; }
}
