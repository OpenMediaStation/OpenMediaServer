using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class StreamingService(ILogger<StreamingService> logger, IInventoryService inventoryService)
    : IStreamingService
{
    public async Task<Stream?> GetMediaStream(Guid id, string category)
    {
        logger.LogTrace("Streaming in category: {Category} id: {Id}", category, id);

        var item = await inventoryService.GetItem<InventoryItem>(id, category);

        if (item == null)
        {
            logger.LogWarning("Item not found in category while streaming");

            return null;
        }

        var stream = new FileStream(item.Path, FileMode.Open);

        return stream;
    }

    public async Task<IResult> GetTranscodedMediaStream(Guid id, string category, HttpRequest request, HttpResponse response)
    {
        var item = await inventoryService.GetItem<InventoryItem>(id, category);
        
        if(item == null)
            throw new Exception("Requested Item not found in category while prepare transcoding");
        
        var ffprobeStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -select_streams v:0 -show_entries packet=pts_time,flags -skip_frame nokey -of json \"{item.Path}\"",
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
