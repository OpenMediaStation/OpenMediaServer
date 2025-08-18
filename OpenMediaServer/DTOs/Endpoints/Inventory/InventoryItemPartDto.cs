namespace OpenMediaServer.DTOs.Endpoints.Inventory;

public class InventoryItemPartDto
{
    public Guid Id { get; set; }
    public string? Path { get; set; }
    public Guid? FileInfoId { get; set; }
    public string? Name { get; set; }

    public int? PrimaryIdentifier { get; set; }
    public int? SecondaryIdentifier { get; set; }
}