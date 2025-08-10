using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Inventory;

public class InventoryItemPart
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(InventoryItemVersion)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid InventoryItemVersionId { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Path { get; set; }
    
    [Column(TypeName = "UUID")]
    public Guid? FileInfoId { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Name { get; set; }

    [Column(TypeName = "INTEGER")]
    public int? PrimaryIdentifier { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public int? SecondaryIdentifier { get; set; }
}
