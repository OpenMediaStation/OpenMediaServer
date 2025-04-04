using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Endpoints;

public class ImageEndpoints(ILogger<ImageEndpoints> logger, IImageService imageService) : IImageEndpoints
{
    private readonly ILogger<ImageEndpoints> _logger = logger;
    private readonly IImageService _imageService = imageService;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/images").RequireAuthorization();

        group.MapGet("/{category}/{metadataId}/{type}", GetImage);
    }

    public IResult GetImage(string category, Guid metadataId, string type, int? width, int? height)
    {
        var path = _imageService.GetPath(category, metadataId, type, width, height);
        var extension = path?.Split('.').LastOrDefault();
        var stream = _imageService.GetImageStream(path);

        if (stream != null && extension != null)
        {
            return Results.Stream(stream, contentType: MimeTypeHelper.GetMimeType(extension));
        }
        else
        {
            return Results.NotFound("Id not found in images");
        }
    }
}