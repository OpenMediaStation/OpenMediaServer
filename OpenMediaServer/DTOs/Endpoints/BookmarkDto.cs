namespace OpenMediaServer.DTOs.Endpoints;

public class BookmarkDto
{
    public Guid? Id { get; set; }
    public int? PositionInSeconds { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? PageNumber { get; set; }
}