using System;

namespace OpenMediaServer.Models;

public class Movie : InventoryItem
{
    public override string Category
    {
        get { return "Movie"; }
    }
}
