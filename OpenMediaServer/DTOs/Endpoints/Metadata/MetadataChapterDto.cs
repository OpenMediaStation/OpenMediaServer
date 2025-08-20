namespace OpenMediaServer.DTOs.Endpoints.Metadata;

public class MetadataChapterDto
{
    public string? Title { get; set; }
    public uint? StartTimeInSeconds { get; set; }
    public uint? EndTimeInSeconds { get; set; }
}