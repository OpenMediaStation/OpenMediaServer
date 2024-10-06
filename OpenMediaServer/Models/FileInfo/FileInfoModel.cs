using System;

namespace OpenMediaServer.Models.FileInfo;

public class FileInfoModel
{
    public Guid Id { get; set; }

    /// <summary>
    /// Referring to the version id
    /// </summary>
    public Guid ParentId { get; set; }

    public string ParentCategory { get; set; }

    public MediaData? MediaData { get; set; }
}
