using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database.BoardObjects;

public class BoardQuoteRevision
{
    [Key]
    [Column(TypeName = "nvarchar(42)")]
    public int Id { get; set; }
    
    [Column(TypeName = "bit")]
    public bool IsAnonymousQuote { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string SettlementPlace { get; set; } = string.Empty;

    [Column(TypeName = "bit")]
    public bool ShowIfTheBest { get; set; }

    [Column(TypeName = "timestamp")]
    public byte[] RowVersion { get; set; } = [];
    
    [Column(TypeName = "nvarchar(12)")]
    public string? CounterpartyId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string Comment { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(64)")]
    public string ClientCode { get; set; } = string.Empty;

    [Column(TypeName = "bit")]
    public bool IsIndicative { get; set; }

    [Column(TypeName = "decimal(18,5)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,5)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "nvarchar(3)")]
    public string PriceCurrencyId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,5)")]
    public decimal NominalValue => Price*Quantity;

    [Column(TypeName = "decimal(18,5)")]
    public decimal NominalValueRub => Price*Quantity*(ExchangeRate!=0?ExchangeRate:1);

    [Column(TypeName = "nvarchar(3)")]
    public string NominalCurrencyId { get; set; } = string.Empty;

    [Column(TypeName = "int")]
    public int TimeToLive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpirationDate { get; set; }

    [Column(TypeName = "nvarchar(64)")]
    public string DeliveryMethod { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime SettlementDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DeliveryDate { get; set; }

    [Column(TypeName = "nvarchar(3)")]
    public string SettlementCurrencyId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,5)")]
    public decimal ExchangeRate { get; set; }

    [Column(TypeName = "int")]
    public int Number { get; set; }

    [Column(TypeName = "int")]
    public int CreatedById { get; set; }

    [Column(TypeName = "nvarchar(32)")]
    public string State { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(42)")]
    public string DocumentId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string DescriptionRus => $"{(Direction==-1?"Продажа":"Покупка")} {Price} / {Quantity}";

    [Column(TypeName = "nvarchar(max)")]
    public string DescriptionEng => $"{(Direction==-1?"Sell":"Buy")} {Price} / {Quantity}";

    [Column(TypeName = "nvarchar(max)")]
    public string? ErrorCode { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? ErrorTextRus { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? ErrorTextEng { get; set; } = string.Empty;

    [Column(TypeName = "int")]
    public int XmlId { get; set; }
    
    public BoardXml Xml { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreationTime { get; set; }

    [Column(TypeName = "int")]
    public int Direction { get; set; }

    [Column(TypeName = "bit")]
    public bool? IsValid { get; set; }

    [Column(TypeName = "decimal(18,5)")]
    public decimal? StandardPrice { get; set; }

    [Column(TypeName = "xml")]
    public string? ProductSpecificParams { get; set; } = string.Empty;

    [Column(TypeName = "bit")]
    public bool IsPartialExecution { get; set; }

    [Column(TypeName = "bit")]
    public bool IsInformationQuote { get; set; }
}