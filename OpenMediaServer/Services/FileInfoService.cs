using FFMpegCore;
using FFMpegCore.Exceptions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.FileInfo;

namespace OpenMediaServer.Services;

public class FileInfoService : IFileInfoService
{
    private readonly ILogger<FileInfoService> _logger;
    private readonly IFileSystemRepository _fileSystemRepository;

    public FileInfoService(ILogger<FileInfoService> logger, IFileSystemRepository fileSystemRepository)
    {
        _logger = logger;
        _fileSystemRepository = fileSystemRepository;
    }

    public async Task<FileInfoModel?> CreateFileInfo(string path, Guid parentId, string parentCategory)
    {
        var fileInfos = await ListFileInfo(parentCategory);

        IMediaAnalysis? mappingInput;

        try
        {
            mappingInput = await FFProbe.AnalyseAsync(path);
        }
        catch (FFMpegException ffmEx)
        {
            _logger.LogWarning(ffmEx, "FileInfo could not be generated");
            return null;
        }

        FileInfoModel fileInfo = MapFileInfo(parentId, parentCategory, mappingInput);

        fileInfos = fileInfos.Append(fileInfo);

        await _fileSystemRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "fileInfo", parentCategory) + ".json", fileInfos);

        return fileInfo;
    }

    public async Task<IEnumerable<FileInfoModel>> ListFileInfo(string category)
    {
        var metadatas = await _fileSystemRepository.ReadObject<IEnumerable<FileInfoModel>>(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json");

        metadatas ??= [];

        return metadatas;
    }

    public async Task<FileInfoModel?> GetFileInfo(string category, Guid id)
    {
        var fileInfos = await _fileSystemRepository.ReadObject<IEnumerable<FileInfoModel>>(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json");

        var metadata = fileInfos?.FirstOrDefault(x => x.Id == id);

        return metadata;
    }

    public async Task DeleteFileInfo(string category, Guid id)
    {
        var fileInfos = await _fileSystemRepository.ReadObject<IEnumerable<FileInfoModel>>(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json");

        fileInfos = fileInfos?.Where(i => i.Id != id);

        await _fileSystemRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json", fileInfos);
    }

    public async Task DeleteFileInfoByParentId(string category, Guid parentId)
    {
        var fileInfos = await _fileSystemRepository.ReadObject<IEnumerable<FileInfoModel>>(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json");

        fileInfos = fileInfos?.Where(i => i.ParentId != parentId);

        await _fileSystemRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "fileInfo", category) + ".json", fileInfos);
    }

    private FileInfoModel MapFileInfo(Guid parentId, string parentCategory, IMediaAnalysis mappingInput)
    {
        var fileInfo = new FileInfoModel
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            ParentCategory = parentCategory,
            MediaData = new MediaData
            {
                Duration = mappingInput.Duration,
                Format = new()
                {
                    Duration = mappingInput.Duration,
                    StartTime = mappingInput.Format.StartTime,
                    FormatName = mappingInput.Format.FormatName,
                    FormatLongName = mappingInput.Format.FormatLongName,
                    StreamCount = mappingInput.Format.StreamCount,
                    ProbeScore = mappingInput.Format.ProbeScore,
                    BitRate = mappingInput.Format.BitRate,
                    Tags = mappingInput.Format.Tags,
                },
                AudioStreams = mappingInput.AudioStreams.Select(i => MapAudioStream(i)).ToList(),
                VideoStreams = mappingInput.VideoStreams.Select(i => MapVideoStream(i)).ToList(),
                SubtitleStreams = mappingInput.SubtitleStreams.Select(i => MapSubtitleStream(i)).ToList(),
                ErrorData = mappingInput.ErrorData,
            }
        };

        if (mappingInput.PrimaryAudioStream != null)
        {
            fileInfo.MediaData.PrimaryAudioStream = MapAudioStream(mappingInput.PrimaryAudioStream);
        }

        if (mappingInput.PrimaryVideoStream != null)
        {
            fileInfo.MediaData.PrimaryVideoStream = MapVideoStream(mappingInput.PrimaryVideoStream);
        }

        if (mappingInput.PrimarySubtitleStream != null)
        {
            fileInfo.MediaData.PrimarySubtitleStream = MapSubtitleStream(mappingInput.PrimarySubtitleStream);
        }

        return fileInfo;
    }

    private Models.FileInfo.AudioStream MapAudioStream(FFMpegCore.AudioStream input)
    {
        return new Models.FileInfo.AudioStream()
        {
            Channels = input.Channels,
            ChannelLayout = input.ChannelLayout,
            SampleRateHz = input.SampleRateHz,
            Profile = input.Profile,

            // MediaStream
            Index = input.Index,
            CodecName = input.CodecName,
            CodecLongName = input.CodecLongName,
            CodecTagString = input.CodecTagString,
            CodecTag = input.CodecTag,
            BitRate = input.BitRate,
            StartTime = input.StartTime,
            Duration = input.Duration,
            Language = input.Language,
            Disposition = input.Disposition,
            Tags = input.Tags,
            BitDepth = input.BitDepth,
        };
    }

    private Models.FileInfo.SubtitleStream MapSubtitleStream(FFMpegCore.SubtitleStream input)
    {
        return new Models.FileInfo.SubtitleStream()
        {
            // MediaStream
            Index = input.Index,
            CodecName = input.CodecName,
            CodecLongName = input.CodecLongName,
            CodecTagString = input.CodecTagString,
            CodecTag = input.CodecTag,
            BitRate = input.BitRate,
            StartTime = input.StartTime,
            Duration = input.Duration,
            Language = input.Language,
            Disposition = input.Disposition,
            Tags = input.Tags,
            BitDepth = input.BitDepth,
        };
    }

    private Models.FileInfo.VideoStream MapVideoStream(FFMpegCore.VideoStream input)
    {
        return new Models.FileInfo.VideoStream()
        {
            AvgFrameRate = input.AvgFrameRate,
            BitsPerRawSample = input.BitsPerRawSample,
            DisplayAspectRatio = input.DisplayAspectRatio,
            SampleAspectRatio = input.SampleAspectRatio,
            Profile = input.Profile,
            Width = input.Width,
            Height = input.Height,
            FrameRate = input.FrameRate,
            PixelFormat = input.PixelFormat,
            Rotation = input.Rotation,
            AverageFrameRate = input.AverageFrameRate,

            // MediaStream
            Index = input.Index,
            CodecName = input.CodecName,
            CodecLongName = input.CodecLongName,
            CodecTagString = input.CodecTagString,
            CodecTag = input.CodecTag,
            BitRate = input.BitRate,
            StartTime = input.StartTime,
            Duration = input.Duration,
            Language = input.Language,
            Disposition = input.Disposition,
            Tags = input.Tags,
            BitDepth = input.BitDepth,
        };
    }
}
