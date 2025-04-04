using System.Text.RegularExpressions;
using Blurhash.ImageSharp;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
            file = _fileSystemRepository.GetFiles(directoryPath, type + ".*").FirstOrDefault(f => Regex.IsMatch(Path.GetFileName(f), @"^[^.]+\.[^.]+$"));
        }

        var extension = file?.Split('.').LastOrDefault();

        if (file == null || extension == null)
        {
            return null;
        }

        return file;
    }

    public string? CreateBlurHash(byte[]? bytes)
    {
        if(bytes == null || bytes.Length == 0)
            return null;

        try
        {
            var img = Image.Load<Rgba32>(bytes);
            return Blurhasher.Encode(img,5,5);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to encode blurhash: {e.Message}", e);
            return null;
        }
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

    public async Task<string?> WriteImage(byte[]? bytes, string url, string fileName, string category, string id, string? imageType = null)
    {
        if (url == null)
            return null;

        if (bytes == null)
            return null;

        var extension = url.Split(".").LastOrDefault();

        if (imageType != null)
        {
            extension = imageType;
        }

        await _fileSystemRepository.WriteBytes(GetPath(fileName, category, id, extension), bytes);

        if (extension != "svg")
        {
            foreach (var size in _imageSizes)
            {
                await ResizeImage(bytes, size, null, GetPath(fileName, category, id, extension, "w"+size));
                await ResizeImage(bytes, null, size, GetPath(fileName, category, id, extension, "h"+size));
            }
        }

        return $"{Globals.Domain}/images/{category}/{id}/{fileName}";
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
