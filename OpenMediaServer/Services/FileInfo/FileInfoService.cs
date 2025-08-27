using FFMpegCore;
using FFMpegCore.Exceptions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Services.FileInfo;

public class FileInfoService(
    ILogger<FileInfoService> logger,
    IDataRepository dataRepository,
    IMediaDataService mediaDataService,
    IMediaFormatService mediaFormatService,
    IVideoStreamService videoStreamService,
    IAudioStreamService audioStreamService,
    ISubtitleStreamService subtitleStreamService)
    : IFileInfoService
{
    public async Task<FileInfoModel?> CreateFileInfo(string path, Guid parentId, string parentCategory)
    {
        IMediaAnalysis? mappingInput;

        try
        {
            mappingInput = await FFProbe.AnalyseAsync(path);
        }
        catch (FFMpegException ffmEx)
        {
            logger.LogWarning(ffmEx, "FileInfo could not be generated");
            return null;
        }

        FileInfoModel fileInfo = await MapFileInfo(parentId, parentCategory, mappingInput);

        await dataRepository.WriteObject(fileInfo);

        return fileInfo;
    }

    public async Task<IEnumerable<FileInfoModel>> ListFileInfo(string category)
    {
        var metadatas = await dataRepository.ListObjects<FileInfoModel>(fi => fi.ParentCategory == category);

        return metadatas;
    }

    public async Task<FileInfoModel?> GetFileInfo(Guid? id)
    {
        if (id == null) return null;

        var fileInfo = await dataRepository.GetObjectById<FileInfoModel>(id);

        return fileInfo;
    }

    public async Task<IEnumerable<FileInfoModel>?> GetFileInfos(string category, List<Guid> ids)
    {
        //filtering in C# instead of SQL (not working, expression with contains is to complex right now.. maybe working in the future)
        var fileInfos = (await dataRepository.ListObjects<FileInfoModel>()).Where(fi => ids.Contains(fi.Id));

        return fileInfos;
    }

    public async Task DeleteFileInfo(Guid? id)
    {
        var fileInfo = await GetFileInfo(id);

        if (id == null || fileInfo == null)
        {
            logger.LogWarning("FileInfo could not be deleted");

            return;
        }

        var mediaData = await mediaDataService.Get(fileInfo.MediaDataId);

        if (mediaData != null)
        {
            await mediaFormatService.Delete(mediaData.MediaFormatId);

            await subtitleStreamService.Delete(i => i.MediaDataId == mediaData.Id);
            await audioStreamService.Delete(i => i.MediaDataId == mediaData.Id);
            await videoStreamService.Delete(i => i.MediaDataId == mediaData.Id);
            
            await mediaDataService.Delete(fileInfo.MediaDataId);
        }
        
        await dataRepository.DeleteObjectWithFilter<FileInfoModel>(id.Value);
    }

    private async Task<FileInfoModel> MapFileInfo(Guid parentId, string parentCategory, IMediaAnalysis mappingInput)
    {
        var fileInfo = new FileInfoModel
        {
            Id = Guid.NewGuid(),
            ParentCategory = parentCategory,
        };

        var mediaData = await mediaDataService.CreateMediaData(mappingInput);

        fileInfo.MediaDataId = mediaData.Id;

        return fileInfo;
    }
}