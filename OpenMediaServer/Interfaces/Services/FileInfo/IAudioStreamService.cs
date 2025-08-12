using FFMpegCore;
using AudioStream = OpenMediaServer.Models.FileInfo.AudioStream;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IAudioStreamService  : ITableBaseService<AudioStream>
{
    Task<AudioStream?> CreateAudioStreams(IMediaAnalysis mappingInput, Guid mediaDataId);
}