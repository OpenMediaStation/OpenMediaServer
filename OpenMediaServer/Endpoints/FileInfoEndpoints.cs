using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.DTOs.Endpoints.FileInfo;
using OpenMediaServer.Extensions.Mapping;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Endpoints;

public class FileInfoEndpoints(ILogger<FileInfoEndpoints> logger, IFileInfoService fileInfoService, IMediaDataService mediaDataService, IMediaFormatService mediaFormatService, IAudioStreamService audioStreamService, IVideoStreamService videoStreamService, ISubtitleStreamService subtitleStreamService) : IFileInfoEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/fileInfo");

        group.MapGet("list", ListFileInfos).RequireAuthorization();
        group.MapGet("", GetFileInfo).RequireAuthorization();
        group.MapGet("/batch", GetFileInfos).RequireAuthorization();
    }

    public async Task<IResult> ListFileInfos(string category)
    {
        var fileInfos = await fileInfoService.ListFileInfo(category);

        List<FileInfoDto> fileInfoDtos = [];
        
        foreach (var fileInfo in fileInfos)
        {
            fileInfoDtos.Add(await ToFileInfoDto(fileInfo));
        }

        return Results.Ok(fileInfoDtos);
    }

    public async Task<IResult> GetFileInfo(string category, Guid id)
    {
        var fileInfo = await fileInfoService.GetFileInfo(id);

        return Results.Ok(await ToFileInfoDto(fileInfo));
    }

    public async Task<IResult> GetFileInfos(string category, [FromQuery] Guid[] ids)
    {
        var fileInfos = await fileInfoService.GetFileInfos(category, ids.ToList());

        List<FileInfoDto> fileInfoDtos = [];
        
        foreach (var fileInfo in fileInfos)
        {
            fileInfoDtos.Add(await ToFileInfoDto(fileInfo));
        }

        return Results.Ok(fileInfoDtos);
    }

    private async Task<FileInfoDto> ToFileInfoDto(FileInfoModel fileInfo)
    {
        var mediaData = await mediaDataService.Get(fileInfo.MediaDataId);
        var mediaFormat = await mediaFormatService.Get(mediaData?.MediaFormatId);
        var primaryAudioStream = await audioStreamService.Get(mediaData?.PrimaryAudioStreamId);
        var primaryVideoStream = await videoStreamService.Get(mediaData?.PrimaryVideoStreamId);
        var primarySubtitleStream = await subtitleStreamService.Get(mediaData?.PrimarySubtitleStreamId);
        var audioStreams = await audioStreamService.List(i => i.MediaDataId == mediaData.Id);
        var videoStreams = await videoStreamService.List(i => i.MediaDataId == mediaData.Id);
        var subtitleStreams = await subtitleStreamService.List(i => i.MediaDataId == mediaData.Id);
            
            
        return fileInfo.ToDto(mediaData, mediaFormat, primaryAudioStream, audioStreams, primarySubtitleStream, subtitleStreams, primaryVideoStream, videoStreams);
    }
}
