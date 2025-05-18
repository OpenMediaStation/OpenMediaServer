using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models;

public class Bookmark
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid? Id { get; set; }
    
    
    [Column(Order = 1, TypeName = "TEXT")]
    public string? UserId { get; set; }

    [Column(Order = 2, TypeName = "TEXT")]
    public string? Category { get; set; }
    
    [Column(Order = 3, TypeName = "UUID")] 
    [ForeignKey(nameof(Metadata.MetadataModel)+"(Id)")]
    public Guid? InventoryItemId { get; set; }
    
    public int? PositionInSeconds { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? PageNumber { get; set; }
}