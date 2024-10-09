using System;
using Microsoft.Extensions.FileProviders;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class DiscoveryBookService : IDiscoveryBookService
{
    private readonly ILogger<DiscoveryBookService> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryBookService(ILogger<DiscoveryBookService> logger, IFileInfoService fileInfoService, IInventoryService inventoryService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
    }

    public async Task CreateBook(string path)
    {
        _logger.LogTrace("Creating book for path: {Path}", path);
    }
}
