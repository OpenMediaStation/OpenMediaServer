using System;
using OpenMediaServer.Interfaces.Services;

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

    public async Task<Stream?> GetMediaStream(Guid id, string category)
    {
        _logger.LogDebug("Streaming in category: {Category} id: {Id}", category, id);

        var item = await _inventoryService.GetItem(id, category);

        if (item == null)
        {
            _logger.LogWarning("Item not found in category while streaming");

            return null;
        }

        var stream = new FileStream(item.Path, FileMode.Open);

        return stream;
    }
}
