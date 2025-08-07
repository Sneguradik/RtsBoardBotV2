using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database.BoardObjects;

public class BoardQuote
{
    [Column(TypeName = "nvarchar(42)")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = string.Empty;

    [Column(TypeName = "int")]
    public int CreatedById { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationTime { get; set; }

    [Column(TypeName = "int")]
    public int CurrentRevisionId { get; set; }
    
    public BoardQuoteRevision CurrentRevision { get; set; } = null!;

    [Column(TypeName = "nvarchar(42)")]
    public string? FrontTradeId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(42)")]
    public string EId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(64)")]
    public string InstrumentId { get; set; } = string.Empty;
    
    public BoardInstrument Instrument { get; set; } = null!;

    [Column(TypeName = "bit")]
    public bool IsDynamic { get; set; }

    [Column(TypeName = "int")]
    public int? LockOwnerId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LockTime { get; set; }

    [Column(TypeName = "nvarchar(12)")]
    public string PartyId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(42)")]
    public string? QuoteReplyId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(42)")]
    public string? QuoteRequestId { get; set; } = string.Empty;
}