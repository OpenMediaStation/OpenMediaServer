namespace OpenMediaServer.DTOs.Endpoints.Inventory;

public class InventoryItemVersionDto
{
    public Guid Id { get; set; }
    public string? Path { get; set; }
    public Guid? FileInfoId { get; set; }
    public string? Name { get; set; }
    public IEnumerable<InventoryItemPartDto>? Parts { get; set; }
}