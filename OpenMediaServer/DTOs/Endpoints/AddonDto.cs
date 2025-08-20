namespace OpenMediaServer.DTOs.Endpoints;

public record AddonDto
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public string Category { get; set; }
    public AddonSubtitleDto? Subtitle {get; set;}
}