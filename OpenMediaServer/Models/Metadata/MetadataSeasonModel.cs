using System;

namespace OpenMediaServer.Models.Metadata;

public class MetadataSeasonModel
{
    public string? Poster { get; set; }
    public DateTime? AirDate { get; set; }
    public int? EpisodeCount { get; set; }
    public string? Overview { get; set; }
    public double? Popularity { get; set; }
}

