using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Interfaces.Services;

public interface IFileInfoService
{
    Task<FileInfoModel?> CreateFileInfo(string path, Guid parentId, string parentCategory);
    Task<IEnumerable<FileInfoModel>> ListFileInfo(string category);
    Task<FileInfoModel?> GetFileInfo(string category, Guid id);
    Task DeleteFileInfo(string category, Guid id);
    Task DeleteFileInfoByParentId(string category, Guid parentId);
    Task<IEnumerable<FileInfoModel>?> GetFileInfos(string category, List<Guid> ids);
}
