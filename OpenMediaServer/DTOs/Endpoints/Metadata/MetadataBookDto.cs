namespace OpenMediaServer.DTOs.Endpoints.Metadata;

public class MetadataBookDto
{
    public IEnumerable<string>? Authors { get; set; }
    public string? Publisher { get; set; }
    public string? PublishedDate { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string? Language { get; set; }
    public string? Thumbnail { get; set; }
    public string? ThumbnailBlurHash { get; set; }
}