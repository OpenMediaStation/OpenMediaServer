using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace OpenMediaServer.Services;

public class ImageService : IImageService
{
    private readonly IConfiguration _configuration;
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly int[] _imageSizes = [150, 300, 500];
    

    public ImageService(IConfiguration configuration, IFileSystemRepository fileSystemRepository)
    {
        _configuration = configuration;
        _fileSystemRepository = fileSystemRepository;
    }

    public string? GetPath(string category, Guid metadataId, string type, int? width, int? height)
    {
        var directoryPath = Path.Combine(Globals.ConfigFolder, "images", category, metadataId.ToString());

        string? file;

        if (width != null)
        {
            width = _imageSizes.Order().LastOrDefault(size => size > width || width > _imageSizes.Max());
            file = _fileSystemRepository.GetFiles(directoryPath, $"{type}.w{width}.*").FirstOrDefault();
        }
        else if (height != null)
        {
            height = _imageSizes.Order().LastOrDefault(size => size > height || height > _imageSizes.Max());
            file = _fileSystemRepository.GetFiles(directoryPath, $"{type}.h{height}.*").FirstOrDefault();
        }
        else
        {
            file = _fileSystemRepository.GetFiles(directoryPath, type + ".*").Where(file => Regex.IsMatch(Path.GetFileName(file), @"^[^.]+\.[^.]+$")).FirstOrDefault();
        }

        var extension = file?.Split('.').LastOrDefault();

        if (file == null || extension == null)
        {
            return null;
        }

        return file;
    }

    public Stream? GetImageStream(string? path)
    {
        if (path == null)
        {
            return null;
        }

        var stream = _fileSystemRepository.GetStream(path);

        return stream;
    }

    public async Task WriteImage(byte[]? bytes, string url, string fileName, string category, string id)
    {
        if (url == null)
            return;

        if (bytes == null)
            return;

        var extension = url.Split(".").LastOrDefault();

        await _fileSystemRepository.WriteBytes(GetPath(fileName, category, id, extension), bytes);

        if (extension != "svg")
        {
            foreach (var size in _imageSizes)
            {
                await ResizeImage(bytes, size, null, GetPath(fileName, category, id, extension, "w"+size));
                await ResizeImage(bytes, null, size, GetPath(fileName, category, id, extension, "h"+size));
            }
        }
    }

    private static string GetPath(string fileName, string category, string id, string? extension, string? addon = null)
    {
        string fullFileName;

        if (!string.IsNullOrWhiteSpace(addon))
        {
            fullFileName = fileName + "." + addon + "." + extension;
        }
        else
        {
            fullFileName = fileName + "." + extension;
        }

        var path = Path.Combine(Globals.ConfigFolder, "images", category, id, fullFileName);
        return path;
    }

    private async Task ResizeImage(byte[] bytes, int? width, int? height, string path)
    {
        if (width == null && height == null)
        {
            return;
        }

        Image image = Image.Load(bytes);

        int originalWidth = image.Width;
        int originalHeight = image.Height;

        float aspectRatio = (float)originalWidth / originalHeight;

        if (width == null)
        {
            width = (int)(height * aspectRatio);
        }
        else if (height == null)
        {
            height = (int)(width / aspectRatio);
        }

        image.Mutate(x => x.Resize((int)width, (int)height));

        await image.SaveAsPngAsync(path);
    }
}
