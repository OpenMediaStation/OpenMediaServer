using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class FileInfoModel
{
    [Key]
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    /// <summary>
    /// Referring to the version id
    /// </summary>
    [Column(TypeName = "UUID")]
    public Guid ParentId { get; set; }

    [Column(TypeName = "TEXT")]
    public required string ParentCategory { get; set; }
    
    [ForeignKey(nameof(MediaData)+"(Id)")]
    [Column(TypeName = "UUID")]
    public Guid MediaDataId { get; set; }
}
