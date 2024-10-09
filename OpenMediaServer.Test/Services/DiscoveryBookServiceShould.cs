using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryBookServiceShould
{
    private readonly ILogger<DiscoveryBookService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryBookService _inventoryBookService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryBookServiceShould()
    {
        _logger = Substitute.For<ILogger<DiscoveryBookService>>();
        _storageRepository = new FileSystemRepoMock();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryBookService = new DiscoveryBookService(_logger, _fileInfoService, _inventoryService);
    }

    [Theory]
    [InlineData("/media/Books/practicalsocialengineering.epub", "practicalsocialengineering", null)]
    [InlineData("/media/Books/Practical Socialengineering.epub", "Practical Socialengineering", null)]
    [InlineData("/media/Books/Die Tribute von Panem/Die Tribute Von Panem. Gefährliche Liebe/Die Tribute Von Panem. Gefährliche Liebe.epub", "Die Tribute Von Panem. Gefährliche Liebe", "/media/Books/Die Tribute von Panem/Die Tribute Von Panem. Gefährliche Liebe")]
    [InlineData("/media/Books/Die Tribute von Panem/Die Tribute von Panem X - Das Lied von Vogel und Schlange/Die Tribute von Panem X - Das Lied von Vogel und Schlange.epub", "Die Tribute von Panem X - Das Lied von Vogel und Schlange", "/media/Books/Die Tribute von Panem/Die Tribute von Panem X - Das Lied von Vogel und Schlange")]
    [InlineData("/media/Books/Hjärta serien/Ishjärta/Ishjärta.epub", "Ishjärta", "/media/Books/Hjärta serien/Ishjärta")]
    [InlineData("/media/Books/Quality Land/QualityLand 2.0 Kikis Geheimnis/QualityLand 2.0 Kikis Geheimnis.m4b", "QualityLand 2.0 Kikis Geheimnis", "/media/Books/Quality Land/QualityLand 2.0 Kikis Geheimnis")]
    public async Task Create_FirstItemBook(string path, string title, string? folderPath)
    {
        // Arrange
        var paths = new List<string>
        {
            path
        };

        // Act
        await _inventoryBookService.CreateBook(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Book>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Book");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}
