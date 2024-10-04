using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class StreamingService : IStreamingService
{
    private readonly ILogger<StreamingService> _logger;
    private readonly IInventoryService _inventoryService;

    public StreamingService(ILogger<StreamingService> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task<Stream?> GetMediaStream(Guid id, string category, Guid? versionId = null)
    {
        _logger.LogTrace("Streaming in category: {Category} id: {Id}", category, id);

        var item = await _inventoryService.GetItem<InventoryItem>(id, category);

        if (item == null)
        {
            _logger.LogWarning("Item not found in category while streaming");

            return null;
        }

        Stream stream;

        if (versionId == null)
        {
            var playVersion = item.Versions?.FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }

            stream = new FileStream(playVersion.Path, FileMode.Open);
        }
        else
        {
            var playVersion = item.Versions?.Where(i => i.Id == versionId).FirstOrDefault();

            if (playVersion == null)
            {
                return null;
            }

            stream = new FileStream(playVersion.Path, FileMode.Open);
        }

        return stream;
    }

    public async Task<IResult> GetTranscodedMediaStream(Guid id, string category, HttpRequest request, HttpResponse response)
    {
        // TODO Live transcoding if possible
        throw new NotImplementedException();
    }
}
