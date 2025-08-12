using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models.FileInfo;

public class SubtitleStream : MediaStream
{
    [Column(TypeName = "TEXT")] 
    public string Category { get; set; } = "SubtitleStream";
}
