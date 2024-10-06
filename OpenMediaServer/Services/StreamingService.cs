using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class StreamingService : IStreamingService
{
    private readonly ILogger<StreamingService> _logger;
    private readonly IInventoryService _inventoryService;

    public StreamingService(ILogger<StreamingService> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task<Stream?> GetMediaStream(Guid id, string category, Guid? versionId = null)
    {
        _logger.LogTrace("Streaming in category: {Category} id: {Id}", category, id);

        var item = await _inventoryService.GetItem<InventoryItem>(id, category);

        if (item == null)
        {
            _logger.LogWarning("Item not found in category while streaming");

            return null;
        }

        Stream stream;

        if (versionId == null)
        {
            var playVersion = item.Versions?.FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }

            stream = new FileStream(playVersion.Path, FileMode.Open);
        }
        else
        {
            var playVersion = item.Versions?.Where(i => i.Id == versionId).FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }

            stream = new FileStream(playVersion.Path, FileMode.Open);
        }

        return stream;
    }

    public async Task<IResult> GetTranscodedMediaStream(Guid id, string category, HttpRequest request, HttpResponse response, Guid? versionId = null)
    {
        var item = await _inventoryService.GetItem<InventoryItem>(id, category);
        
        if(item == null)
            throw new Exception("Requested Item not found in category while prepare transcoding");

        var path = "";
        
        if (versionId == null)
        {
            var playVersion = item.Versions?.FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }
            path = playVersion.Path;
        }
        else
        {
            var playVersion = item.Versions?.Where(i => i.Id == versionId).FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }

            path = playVersion.Path;
        }
        
        var ffprobeStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -select_streams v:0 -show_entries packet=pts_time,flags -skip_frame nokey -of json \"{path}\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        
        var ffprobe = Process.Start(ffprobeStartInfo) ?? throw new Exception($"Failed to start ffprobe process");
        
        var keyframeOutput = await ffprobe.StandardOutput.ReadToEndAsync();
        await ffprobe.WaitForExitAsync();
        var keyframeTimeStamps = JObject.Parse(keyframeOutput)["packets"].Where(p=>(string)p["flags"] == "K_").Select(p=>(double)p["pts_time"]).ToArray();

        var segmentTimeStamps = GetSegmentTimes(10.0).ToArray();
        
        var segmentMaxDuration = segmentTimeStamps.Max(sTS => sTS.Duration);
        
        var sb = new StringBuilder();
        sb.AppendLine("#EXTM3U");
        sb.AppendLine("#EXT-X-VERSION:3");
        sb.AppendLine($"#EXT-X-TARGETDURATION:{segmentMaxDuration}");
        sb.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");
        sb.AppendLine("#EXT-X-PLAYLIST-TYPE:VOD");
        sb.AppendLine("#EXT-X-ALLOW-CACHE:YES");
        sb.AppendLine("");
        sb.AppendLine("#EXT-X-APPLICATION-STATUS:1");

        foreach (var segment in segmentTimeStamps)
        {
            sb.AppendLine($"#EXTINF:{segment.Duration},");
            sb.AppendLine($"http://localhost:32768/stream/Movie/e401c3ba-4d55-424e-b286-bd71b7964ccd/segments/segment{segment.Start}-{segment.End}.ts");
        }

        sb.AppendLine("#EXT-X-ENDLIST");

        return Results.Text(sb.ToString(), contentType: "application/vnd.apple.mpegurl");

        IEnumerable<(double Start, double End, double Duration)> GetSegmentTimes(double minSegmentDuration = 10.0)
        {
            var start = keyframeTimeStamps.First();
            foreach (var timestamp in keyframeTimeStamps.Skip(1))
            {
                if (timestamp - start >= minSegmentDuration)
                {
                    yield return (start, timestamp, timestamp - start);
                    start = timestamp;
                }
            }
            var lastTimeStamp = keyframeTimeStamps.Last();
            if (lastTimeStamp > start)
            {
                yield return (start, lastTimeStamp, lastTimeStamp - start);
            }
        }
    }
}
