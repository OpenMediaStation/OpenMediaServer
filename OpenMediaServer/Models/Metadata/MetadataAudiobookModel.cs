using System;

namespace OpenMediaServer.Models.Metadata;

public class MetadataAudiobookModel
{
    public IEnumerable<string>? Authors { get; set; }
    public string? Publisher { get; set; }
    public string? PublishedDate { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? Thumbnail { get; set; }
    public string? ThumbnailBlurHash { get; set; }
    public IEnumerable<MetadataAudiobookChapter>? Chapters { get; set; }
}

public class MetadataAudiobookChapter
{
    public string? Title { get; set; }
    public uint? StartTimeInSeconds { get; set; }
    public uint? EndTimeInSeconds { get; set; }
}
