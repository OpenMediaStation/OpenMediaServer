using FFMpegCore;
using FFMpegCore.Exceptions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Services.FileInfo;

public class FileInfoService(ILogger<FileInfoService> logger, IDataRepository dataRepository, IMediaDataService mediaDataService)
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
        var metadatas = await dataRepository.ListObjects<FileInfoModel>(fi => fi.ParentCategory == category);;

        return metadatas;
    }

    public async Task<FileInfoModel?> GetFileInfo(string category, Guid id)
    {
        var fileInfo = await dataRepository.GetObjectById<FileInfoModel>(id, fi => fi.ParentCategory == category);
        
        return fileInfo;
    }

    public async Task<IEnumerable<FileInfoModel>?> GetFileInfos(string category, List<Guid> ids)
    {
        //filtering in C# instead of SQL (not working, expression with contains is to complex right now.. maybe working in the future)
        var fileInfos = (await dataRepository.ListObjects<FileInfoModel>()).Where(fi => ids.Contains(fi.Id)); 
        
        return fileInfos;
    }

    public async Task DeleteFileInfo(string category, Guid id)
    {
        await dataRepository.DeleteObjectWithFilter<FileInfoModel>(id, (fi => fi.ParentCategory == category));
    }

    public async Task DeleteFileInfoByParentId(string category, Guid parentId)
    {
        var matchingFileInfos = await dataRepository.ListObjects<FileInfoModel>(fi => fi.ParentCategory == category && fi.ParentId == parentId);
        
        await dataRepository.DeleteObjects(matchingFileInfos);
    }

    private async Task<FileInfoModel> MapFileInfo(Guid parentId, string parentCategory, IMediaAnalysis mappingInput)
    {
        var fileInfo = new FileInfoModel
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            ParentCategory = parentCategory,
        };

        var mediaData = await mediaDataService.CreateMediaData(mappingInput);
        
        fileInfo.MediaDataId = mediaData.Id;
        
        return fileInfo;
    }
}
