using System.Diagnostics;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class StreamingEndpoints : IStreamingEndpoints
{
    private readonly ILogger<StreamingEndpoints> _logger;
    private readonly IStreamingService _streamingService;
    private readonly IInventoryService _inventoryService;

    public StreamingEndpoints(ILogger<StreamingEndpoints> logger, IStreamingService streamingService, IInventoryService inventoryService)
    {
        _logger = logger;
        _streamingService = streamingService;
        _inventoryService = inventoryService;
    }

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/stream");

        group.MapGet("/{category}/{id}", StreamContent);
        group.MapGet("/{category}/{id}/playlist.m3u8", StreamContent);
        
        group.MapGet("/{category}/{id}/segments/segment{segmentStart}-{segmentEnd}.ts", StreamSegment);
    }

    public async Task<IResult> StreamSegment(HttpContext context, double segmentStart, double segmentEnd, string category, Guid id, Guid? versionId = null)
    {
        Process? ffmpeg = null;
        try
        {
            var path = "";
            var item = await _inventoryService.GetItem<InventoryItem>(id, category);
            
            if(item == null)
                throw new ApplicationException($"Item with id {id} was not found");

            if (versionId != null)
            {
                path = item.Versions?.FirstOrDefault(v => v.Id == versionId)?.Path;
            }
            else
            {
                path = item.Versions?.FirstOrDefault()?.Path;
            }
            
            if(string.IsNullOrWhiteSpace(path))
                throw new ApplicationException("version not found");
            
            var startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                // Arguments = $"-i pipe:0 -ss {segmentStart} -t {segmentDuration} -force_key_frames \"expr:gte(t,n_forced*{segmentDuration})\" -map 0:v -map 0:a -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                // Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart-2: 0.0)} -i pipe:0 -vf \"trim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -an -map 0:v -map 0:a -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                // Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart-2: 0.0)} -i \"{path}\" -vf \"trim=start={segmentStart}:end={segmentEnd}\" -af \"atrim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -analyzeduration 100M -probesize 100M -c:v libx264 -c:a aac -f mpegts pipe:1",
                Arguments = $"-v error -copyts -ss {(segmentStart >= 2 ? segmentStart-2: 0.0)} -i \"{path}\" -vf \"trim=start={segmentStart}:end={segmentEnd}\" -af \"atrim=start={segmentStart}:end={segmentEnd}\" -threads 4 -force_key_frames \"expr:gte(t,n_forced*2)\" -preset fast -analyzeduration 100M -probesize 100M -c:v libx264 -f mpegts pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ffmpeg = Process.Start(startInfo);
            if(ffmpeg == null) return Results.Problem("Failed to start ffmpeg");
            
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
            if(ffmpeg != null)
            {
                if(!ffmpeg.HasExited) 
                    context.RequestAborted.Register(ffmpeg.Kill);
            }
        }
    }

    public async Task<IResult> StreamContent(Guid id, string category, HttpRequest request, HttpResponse response, bool transcode = false, Guid? versionId = null)
    {
        if (transcode)
        {
            return await _streamingService.GetTranscodedMediaStream(id, category, request, response);
        }
        else
        {
            var stream = await _streamingService.GetMediaStream(id, category);

            if (stream == null)
            {
                return Results.NotFound("Id not found in category");
            }

            return Results.Stream(stream, enableRangeProcessing: true, contentType: "video/webm"); // TODO content type
        }
    }
}
