using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IFileInfoService
{
    Task<FileInfoModel?> CreateFileInfo(string path, Guid parentId, string parentCategory);
    Task<IEnumerable<FileInfoModel>> ListFileInfo(string category);
    Task<FileInfoModel?> GetFileInfo(Guid? id);
    Task DeleteFileInfo(Guid? id);
    Task<IEnumerable<FileInfoModel>?> GetFileInfos(string category, List<Guid> ids);
}
