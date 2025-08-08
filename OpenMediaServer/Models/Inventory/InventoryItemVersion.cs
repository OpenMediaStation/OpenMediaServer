using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Models.Inventory;

public class InventoryItemVersion
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Path { get; set; }
    
    [Column(TypeName = "UUID")]
    [ForeignKey(nameof(FileInfoModel)+"(Id)")]
    public Guid? FileInfoId { get; set; }
    
    [Column(TypeName = "TEXT")]
    public string? Name { get; set; }
    
    public IEnumerable<InventoryItemPart>? Parts { get; set; }
}
