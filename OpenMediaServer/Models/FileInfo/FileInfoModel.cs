using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class FileInfoModel
{
    [Key]
    [Column(Order = 0, TypeName = "UUID")]
    public Guid Id { get; set; }

    /// <summary>
    /// Referring to the version id
    /// </summary>
    [Column(Order = 1, TypeName = "UUID")]
    public Guid ParentId { get; set; }

    [Column(Order = 2, TypeName = "TEXT")]
    public required string ParentCategory { get; set; }

    public MediaData? MediaData { get; set; }
}
