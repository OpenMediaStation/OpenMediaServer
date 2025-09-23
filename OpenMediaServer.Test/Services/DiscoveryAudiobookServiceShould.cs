using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Services.Discovery;
using OpenMediaServer.Services.Metadata;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryAudiobookServiceShould
{
  private readonly ILogger<DiscoveryAudiobookService> _logger;
    private readonly DataRepoMock _storageRepository;
    private readonly IFileInfoService _fileInfoService;
    private readonly DiscoveryAudiobookService _inventoryBookService;
    private readonly IInventoryService _inventoryService;
    private readonly IVersionService _versionService;
    private readonly IPartService _partService;
    private readonly IAudioBookMetadataService _audioBookMetadataService;

    public DiscoveryAudiobookServiceShould()
    {
        Setup.Configure();

        _logger = Substitute.For<ILogger<DiscoveryAudiobookService>>();
        _storageRepository = new DataRepoMock();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _partService = Substitute.For<IPartService>();
        _audioBookMetadataService = Substitute.For<IAudioBookMetadataService>();
        _versionService = new VersionServiceMock();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository, Mock.Of<IImageService>());
        _inventoryBookService = new DiscoveryAudiobookService(_logger, _fileInfoService, _inventoryService, Substitute.For<IMetadataService>(), _versionService, _partService, _audioBookMetadataService);
    }

    [Theory]
    [InlineData("/media/Audiobooks/Quality Land/QualityLand 2.0 Kikis Geheimnis/QualityLand 2.0 Kikis Geheimnis.m4b", "QualityLand 2.0 Kikis Geheimnis", "/media/Audiobooks/Quality Land/QualityLand 2.0 Kikis Geheimnis")]
    [InlineData("/media/Audiobooks/Das Känguru-Manifest/Das Känguru-Manifest.m4b", "Das Känguru-Manifest", "/media/Audiobooks/Das Känguru-Manifest")]
    [InlineData("/media/Audiobooks/The Witcher/Ödets Svärd.mp3", "Ödets Svärd", "/media/Audiobooks/The Witcher")]
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
        var resultItem = JsonSerializer.Deserialize<InventoryItem>(resultJson, Globals.JsonOptions);
        var versions = await _versionService.List();

        // Assert
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Audiobook");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        versions.ShouldNotBeNull();
        versions.Count().ShouldBe(1);
        versions.First().Id.ShouldNotBe(Guid.Empty);
        versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }    
    
    [Theory]
    [InlineData("/media/Audiobooks/Asterix bei den Schweizern/Track 1.wav", "Asterix bei den Schweizern", "/media/Audiobooks/Asterix bei den Schweizern/")]
    public async Task MultiPart(string path, string title, string? folderPath)
    {
        // Arrange
        var paths = new List<string>
        {
            path
        };

        // Act
        await _inventoryBookService.CreateAudiobook(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var resultItem = JsonSerializer.Deserialize<InventoryItem>(resultJson, Globals.JsonOptions);
        var versions = await _versionService.List();

        // Assert
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Audiobook");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        versions.ShouldNotBeNull();
        versions.Count().ShouldBe(1);
        versions.First().Id.ShouldNotBe(Guid.Empty);
        versions.First().Path.ShouldBe(folderPath);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}
