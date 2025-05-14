using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models;

public class FavoriteInfo
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid Id { get; set; }
    
    [Column(Order=2, TypeName = "TEXT")]
    public required string UserId { get; set; }
    
    [Column(Order = 1, TypeName = "UUID")]
    [ForeignKey(nameof(InventoryItem) + "(Id)")]
    public Guid InventoryId { get; set; }

    [Column(Order = 3, TypeName = "TEXT")]
    public required string Category { get; set; }
}