using System.ComponentModel.DataAnnotations.Schema;

namespace OpenMediaServer.Models;

public class Season : InventoryItem
{
    [Column(TypeName = "TEXT")]
    public override string Category => "Season";
    public IEnumerable<Guid>? EpisodeIds { get; set; }
    
    [Column(TypeName = "UUID")]
    public Guid ShowId { get; set; }
    
    [Column(TypeName = "INTEGER")]
    public int? SeasonNr { get; set; }
}
