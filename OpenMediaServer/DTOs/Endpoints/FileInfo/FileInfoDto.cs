namespace OpenMediaServer.DTOs.Endpoints.FileInfo;

public class FileInfoDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Referring to the version id
    /// </summary>
    public Guid ParentId { get; set; }

    public string ParentCategory { get; set; }

    public MediaDataDto? MediaData { get; set; }
}