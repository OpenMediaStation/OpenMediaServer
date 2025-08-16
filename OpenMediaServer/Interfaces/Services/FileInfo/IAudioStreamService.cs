using FFMpegCore;
using AudioStream = OpenMediaServer.Models.FileInfo.AudioStream;

namespace OpenMediaServer.Interfaces.Services.FileInfo;

public interface IAudioStreamService  : ITableBaseService<AudioStream>
{
    Task<Guid?> CreateAudioStreams(IMediaAnalysis mappingInput, Guid mediaDataId);
}