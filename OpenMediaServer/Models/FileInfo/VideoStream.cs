using System;

namespace OpenMediaServer.Models.FileInfo;

public class VideoStream : MediaStream
{
    public double AvgFrameRate { get; set; }

    public int BitsPerRawSample { get; set; }

    public (int Width, int Height) DisplayAspectRatio { get; set; }

    public (int Width, int Height) SampleAspectRatio { get; set; }

    public string Profile { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double FrameRate { get; set; }

    public string PixelFormat { get; set; }

    public int Rotation { get; set; }

    public double AverageFrameRate { get; set; }
}
