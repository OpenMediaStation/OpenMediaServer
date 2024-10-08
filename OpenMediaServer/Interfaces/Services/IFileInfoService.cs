using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Interfaces.Services;

public interface IFileInfoService
{
    Task<FileInfoModel?> CreateFileInfo(string path, Guid parentId, string parentCategory);
    Task<IEnumerable<FileInfoModel>> ListFileInfo(string category);
    Task<FileInfoModel?> GetFileInfo(string category, Guid id);
}
