using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Services.Discovery;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryAudiobookServiceShould
{
  private readonly ILogger<DiscoveryAudiobookService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IFileInfoService _fileInfoService;
    private readonly DiscoveryAudiobookService _inventoryBookService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryAudiobookServiceShould()
    {
        Setup.Configure();

        _logger = Substitute.For<ILogger<DiscoveryAudiobookService>>();
        _storageRepository = new FileSystemRepoMock();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository, Mock.Of<IImageService>());
        _inventoryBookService = new DiscoveryAudiobookService(_logger, _fileInfoService, _inventoryService, Substitute.For<IMetadataService>());
    }

    [Theory]
    [InlineData("/media/Audiobooks/Quality Land/QualityLand 2.0 Kikis Geheimnis/QualityLand 2.0 Kikis Geheimnis.m4b", "QualityLand 2.0 Kikis Geheimnis", "/media/Audiobooks/Quality Land/QualityLand 2.0 Kikis Geheimnis")]
    [InlineData("/media/Audiobooks/Das Känguru-Manifest/Das Känguru-Manifest.m4b", "Das Känguru-Manifest", "/media/Audiobooks/Das Känguru-Manifest")]
    public async Task Create_FirstItemBook(string path, string title, string? folderPath)
    {
        // Arrange
        var paths = new List<string>
        {
            path
        };

        // Act
        await _inventoryBookService.CreateAudiobook(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Audiobook>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Audiobook");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}
