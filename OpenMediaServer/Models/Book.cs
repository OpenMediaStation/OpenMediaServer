using System;

namespace OpenMediaServer.Models;

/// <summary>
/// Book
/// </summary>
public class Book : InventoryItem
{
    public override string Category => "Book";
}
