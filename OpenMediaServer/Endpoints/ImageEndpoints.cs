using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Endpoints;

public class ImageEndpoints (ILogger<ImageEndpoints> logger, IFileSystemRepository fileRepo) : IImageEndpoints
{
    private readonly ILogger<ImageEndpoints> _logger = logger;
    private readonly IFileSystemRepository _fileRepo = fileRepo;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/images");

        group.MapGet("/{category}/{metadataId}/{type}", GetImage).RequireAuthorization();
    }

    public IResult GetImage(string category, Guid metadataId, string type)
    {
        var directoryPath = Path.Combine(Globals.ConfigFolder, "images", category, metadataId.ToString());
        
        var file = Directory.GetFiles(directoryPath, type + ".*").FirstOrDefault();
        var extension = file?.Split('.').LastOrDefault();

        if (file == null || extension == null)
        {
            return Results.NotFound("Id not found in images");
        }

        var stream = _fileRepo.GetStream(file);

        if (stream != null)
        {
            return Results.Stream(stream, contentType: MimeTypeHelper.GetMimeType(extension));
        }
        else
        {
            return Results.NotFound("Id not found in images");
        }
    }
}