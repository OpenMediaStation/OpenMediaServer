namespace OpenMediaServer.DTOs.Endpoints.Metadata;

public class MetadataAudiobookDto
{
    public IEnumerable<string>? Authors { get; set; }
    public string? Publisher { get; set; }
    public string? PublishedDate { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? Thumbnail { get; set; }
    public string? ThumbnailBlurHash { get; set; }
    public IEnumerable<MetadataChapterDto>? Chapters { get; set; }
}