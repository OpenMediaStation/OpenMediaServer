using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class StreamingService(ILogger<StreamingService> logger, IInventoryService inventoryService) : IStreamingService
{
    private readonly ILogger<StreamingService> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;

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

    public async Task<IResult> GetTranscodingPlaylist(Guid id, string category, HttpRequest request, HttpResponse response, Guid? versionId = null)
    {
        var item = await _inventoryService.GetItem<InventoryItem>(id, category) ?? throw new Exception("Requested Item not found in category while prepare transcoding");
        var path = "";

        if (versionId == null)
        {
            var playVersion = item.Versions?.FirstOrDefault();

            if (playVersion == null)
            {
                return Results.BadRequest("PlayVersion not found");
            }
            path = playVersion.Path;
        }
        else
        {
            var playVersion = item.Versions?.Where(i => i.Id == versionId).FirstOrDefault();

            if (playVersion == null)
            {
                return Results.BadRequest("PlayVersion not found");
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

        // Parse JSON with System.Text.Json
        var temp1 = JsonNode.Parse(keyframeOutput)?["packets"]?.AsArray();
        var temp2 = temp1?.Where(p => p?["flags"]?.GetValue<string>() == "K_");
        var temp3 = temp2?.Select(p =>
        {
            var ptsTimeString = p?["pts_time"]?.GetValue<string>();

            if (double.TryParse(ptsTimeString, out var ptsTimeValue))
            {
                return ptsTimeValue;  // Return the parsed double value if conversion is successful
            }

            return (double?)null;  // Return null if parsing fails
        });
        
        var keyframeTimestamps = temp3
                    .Where(p => p.HasValue)
                    .Select(p => p.Value)
                    .ToArray();

        if (keyframeTimestamps == null || keyframeTimestamps.Length == 0)
        {
            return Results.BadRequest("No keyframes found");
        }

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
            sb.AppendLine($"{request.Scheme}://{request.Host.Value}/stream/{category}/{id}/segments/segment{segment.Start}-{segment.End}.ts");
        }

        sb.AppendLine("#EXT-X-ENDLIST");

        return Results.Text(sb.ToString(), contentType: "application/vnd.apple.mpegurl");

        IEnumerable<(double Start, double End, double Duration)> GetSegmentTimes(double minSegmentDuration = 10.0)
        {
            var start = keyframeTimestamps.First();
            foreach (var timestamp in keyframeTimestamps.Skip(1))
            {
                if (timestamp - start >= minSegmentDuration)
                {
                    yield return (start, timestamp, timestamp - start);
                    start = timestamp;
                }
            }
            var lastTimeStamp = keyframeTimestamps.Last();
            if (lastTimeStamp > start)
            {
                yield return (start, lastTimeStamp, lastTimeStamp - start);
            }
        }
    }

    public async Task<IResult> GetTranscodingSegment(Guid id, string category, HttpContext context, double segmentStart, double segmentEnd, Guid? versionId = null)
    {
        Process? ffmpeg = null;
        try
        {
            var path = "";
            var item = await _inventoryService.GetItem<InventoryItem>(id, category);

            if (item == null)
                throw new ApplicationException($"Item with id {id} was not found");

            if (versionId != null)
            {
                path = item.Versions?.FirstOrDefault(v => v.Id == versionId)?.Path;
            }
            else
            {
                path = item.Versions?.FirstOrDefault()?.Path;
            }

            if (string.IsNullOrWhiteSpace(path))
                throw new ApplicationException("version not found");

            var startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                // Arguments = $"-i pipe:0 -ss {segmentStart} -t {segmentDuration} -force_key_frames \"expr:gte(t,n_forced*{segmentDuration})\" -map 0:v -map 0:a -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                // Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart-2: 0.0)} -i pipe:0 -vf \"trim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -an -map 0:v -map 0:a -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                // Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart-2: 0.0)} -i \"{path}\" -vf \"trim=start={segmentStart}:end={segmentEnd}\" -af \"atrim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart - 2 : 0.0)} -i \"{path}\" -vf \"trim=start={segmentStart}:end={segmentEnd}\" -af \"atrim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -analyzeduration 100M -probesize 100M -c:v libx264 -f mpegts pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ffmpeg = Process.Start(startInfo);
            if (ffmpeg == null) return Results.Problem("Failed to start ffmpeg");

            context.RequestAborted.Register(ffmpeg.Kill);

            // var preTranscodedStream = await _streamingService.GetMediaStream(id, category, versionId);
            // if (preTranscodedStream == null)
            //     return Results.NotFound("Id not found in category");

            // _ = preTranscodedStream.CopyToAsync(ffmpeg.StandardInput.BaseStream);

            // Setze Cache-Control Header
            context.Response.Headers.CacheControl = "public, max-age=31536000";
            context.Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");

            return Results.Stream(ffmpeg.StandardOutput.BaseStream, /*enableRangeProcessing: true,*/ contentType: "video/MP2T");
        }
        finally
        {
            if (ffmpeg != null)
            {
                if (!ffmpeg.HasExited)
                    context.RequestAborted.Register(ffmpeg.Kill);
            }
        }
    }
}
