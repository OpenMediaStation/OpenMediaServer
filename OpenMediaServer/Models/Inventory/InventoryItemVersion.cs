namespace OpenMediaServer.Models.Inventory;

public class InventoryItemVersion
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public Guid? FileInfoId { get; set; }
    public string? Name { get; set; }
}
