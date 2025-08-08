using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.Progress;

public class Progress
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid? Id { get; set; }
    
    [Column(Order = 1, TypeName = "TEXT")]
    public string? Category { get; set; }
    
    [Column(Order = 2, TypeName = "UUID")]
    public Guid? ParentId { get; set; }
    
    [Column(Order = 3, TypeName = "TEXT")]
    public string? UserId { get; set; }

    [Column(TypeName = "REAL")]
    public float? ProgressPercentage { get; set; }
    
    [Column(TypeName = "INTEGER")] 
    public int? ProgressSeconds { get; set; }

    [Column(TypeName = "INTEGER")] 
    public int? Completions { get; set; }
}
