namespace OpenMediaServer.DTOs.Endpoints.Metadata;

public class MetadataSeasonDto
{
    public string? Poster { get; set; }
    public string? PosterBlurHash { get; set; }
    public DateTime? AirDate { get; set; }
    public int? EpisodeCount { get; set; }
    public string? Overview { get; set; }
}