namespace OpenMediaServer.DTOs
{
    public class GoogleBooksModel
    {
        public string Kind { get; set; }
        public int TotalItems { get; set; }
        public List<Volume>? Items { get; set; }

        public class Volume
        {
            public string Id { get; set; }
            public VolumeInfo? VolumeInfo { get; set; }
        }

        public class VolumeInfo
        {
            public string Title { get; set; }
            public List<string> Authors { get; set; }
            public string Publisher { get; set; }
            public string PublishedDate { get; set; }
            public string Description { get; set; }
            public int PageCount { get; set; }
            public string Language { get; set; }
            public ImageLinks? ImageLinks { get; set; }
        }

        public class ImageLinks
        {
            public string? Thumbnail { get; set; }
        }
    }
}
