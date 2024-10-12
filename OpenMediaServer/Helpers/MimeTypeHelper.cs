namespace OpenMediaServer.Helpers;

public class MimeTypeHelper
{
    // Dictionary to map codecs or formats to their MIME types
    private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "mp4", "video/mp4" },
        { "mkv", "video/x-matroska" },
        { "avi", "video/x-msvideo" },
        { "mmov", "video/quicktime" },
        { "wmv", "video/x-ms-wmv" },
        { "flv", "video/x-flv" },
        { "mp3", "audio/mpeg" },
        { "aac", "audio/aac" },
        { "wav", "audio/wav" },
        { "flac", "audio/flac" },
        { "webm", "video/webm" },
        { "m4b", "audio/mp4" },
        { "epub", "application/epub" },
        { "pdf", "application/pdf" }
    };

    public static string GetMimeType(string codecName)
    {
        if (codecName.Contains(','))
        {
            var codecs = codecName.Split(',');

            foreach (var item in codecs)
            {
                if (MimeTypes.TryGetValue(item, out var mimeType))
                {
                    return mimeType;
                }
            }
        }
        else
        {
            if (MimeTypes.TryGetValue(codecName, out var mimeType))
            {
                return mimeType;
            }
        }

        return "application/octet-stream";
    }
}