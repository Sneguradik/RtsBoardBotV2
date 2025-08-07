using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database.BoardObjects;

public class BoardInstrument
{
    [Key]
    [Column(TypeName = "nvarchar(64)")]
    public string Id { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(64)")]
    public string InstrumentClassId { get; set; } = string.Empty;
    
    public BoardInstrumentClass InstrumentClass { get; set; } = null!;

    [Column(TypeName = "bit")]
    public bool IsActive { get; set; }

    [Column(TypeName = "bit")]
    public bool EnableTrading { get; set; }

    [Column(TypeName = "bit")]
    public bool EnableOrderBook { get; set; }

    [Column(TypeName = "bit")]
    public bool EnableBoard { get; set; }

    [Column(TypeName = "bit")]
    public bool EnableRFQ { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string NameRus { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string NameEng { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(64)")]
    public string ProductTypeId { get; set; } = string.Empty;

    [Column(TypeName = "bit")]
    public bool EnableMarketData { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateAdding { get; set; }
}
