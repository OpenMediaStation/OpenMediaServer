using System;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Endpoints;

public class FileInfoEndpoints(ILogger<FileInfoEndpoints> logger, IFileInfoService fileInfoService) : IFileInfoEndpoints
{
    private readonly ILogger<FileInfoEndpoints> _logger = logger;
    private readonly IFileInfoService _fileInfoService = fileInfoService;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/fileInfo");

        group.MapGet("list", ListFileInfos).RequireAuthorization();
        group.MapGet("", GetFileInfos).RequireAuthorization();
    }

      public async Task<IResult> ListFileInfos(string category)
    {
        var fileInfos = await _fileInfoService.ListFileInfo(category);

        return Results.Ok(fileInfos);
    }

    public async Task<IResult> GetFileInfos(string category, Guid id)
    {
        var fileInfo = await _fileInfoService.GetFileInfo(category, id);

        return Results.Ok(fileInfo);
    }
}
