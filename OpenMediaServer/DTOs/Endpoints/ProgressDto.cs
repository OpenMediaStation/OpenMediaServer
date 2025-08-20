namespace OpenMediaServer.DTOs.Endpoints;

public class ProgressDto
{
    public Guid? Id { get; set; }
    public string? Category { get; set; }
    public Guid? ParentId { get; set; }

    public float? ProgressPercentage { get; set; }
    public int? ProgressSeconds { get; set; }

    public int? Completions { get; set; }
}