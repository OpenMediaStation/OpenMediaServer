using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryShowServiceShould
{
    private readonly ILogger<DiscoveryShowService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryShowService _inventoryMovieShowService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryShowServiceShould()
    {
        _logger = Substitute.For<ILogger<DiscoveryShowService>>();
        _storageRepository = new FileSystemRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryMovieShowService = new DiscoveryShowService(_logger, _fileInfoService, _metadataService, _inventoryService);
    }

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot S01E01", null)]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot S01E01", null)]
    [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "S01E02", null)]
    [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "S01E14", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/S1E3.mp4", "S1E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot S1E3.mp4", "Mr Robot S1E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S04E03.mp4", "Episode name", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name.mp4", "Episode name", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Episode name", null)]
    public async Task CreateFromPaths_FirstItemEpisode(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryMovieShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Movie>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Episode");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}