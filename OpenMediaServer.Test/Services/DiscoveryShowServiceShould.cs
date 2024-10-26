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
    private readonly IDiscoveryShowService _inventoryShowService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryShowServiceShould()
    {
        _logger = Substitute.For<ILogger<DiscoveryShowService>>();
        _storageRepository = new FileSystemRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryShowService = new DiscoveryShowService(_logger, _fileInfoService, _metadataService, _inventoryService);
    }

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot S1E1", null)]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot S1E1", null)]
    [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "Mr Robot S1E2", null)]
    [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "Mr Robot S1E14", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S4E3.mp4", "Mr Robot S4E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Mr Robot S4E3", null)]
    [InlineData("/media/Shows/Babylon Berlin/Season 1/Folge 1 ｜ Staffel 1 (S01⧸E01) [94661334].mp4", "Babylon Berlin S1E1", null)]
    [InlineData("/media/Shows/Cyberpunk - Edgerunners/Season 1/Cyberpunk - Edgerunners - S01E02 - DUAL 1080p WEB H.264 -NanDesuKa (NF)", "Cyberpunk - Edgerunners S1E2", null)]
    public async Task CreateFromPaths_FirstItemEpisode(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First(i => i.Contains("\"Episode\""));
        var result = JsonSerializer.Deserialize<IEnumerable<Episode>>(resultJson);

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

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S04E03.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Babylon Berlin/Season 1/Folge 1 ｜ Staffel 1 (S01⧸E01) [94661334].mp4", "Babylon Berlin", "/media/Shows/Babylon Berlin")]
    [InlineData("/media/Shows/Cyberpunk - Edgerunners/Season 1/Cyberpunk - Edgerunners - S01E02 - DUAL 1080p WEB H.264 -NanDesuKa (NF)", "Cyberpunk - Edgerunners", "/media/Shows/Cyberpunk - Edgerunners")]
    public async Task CreateFromPaths_FirstItemShow(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Show>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Show");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}