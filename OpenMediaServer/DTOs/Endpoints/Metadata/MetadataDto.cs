namespace OpenMediaServer.DTOs.Endpoints.Metadata;

public class MetadataDto
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }

    public string? Title { get; set; }
    public string? Category { get; set; }

    // Specific information
    public MetadataMovieDto? Movie { get; set; }
    public MetadataShowDto? Show { get; set; }
    public MetadataSeasonDto? Season { get; set; }
    public MetadataEpisodeDto? Episode { get; set; }
    public MetadataBookDto? Book { get; set; }
    public MetadataAudiobookDto? Audiobook { get; set; }
}