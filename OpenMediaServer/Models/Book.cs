using System;

namespace OpenMediaServer.Models;

/// <summary>
/// Represents both: Book and audiobook
/// </summary>
public class Book : InventoryItem
{
    public override string Category => "Book";
}
