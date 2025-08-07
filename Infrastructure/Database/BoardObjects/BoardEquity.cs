using Domain.Enums;

namespace Infrastructure.Database.BoardObjects;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BoardEquity
{
    [Key]
    [Column(TypeName = "nvarchar(64)")]
    public string Id { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string IssuerRus { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string IssuerEng { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,5)")]
    public decimal UnitNominal { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string ISIN { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string CFI { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string Regnum { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(3)")]
    public string CurrencyId { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? ExchangeId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,5)")]
    public decimal? CouponRate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Maturity { get; set; }

    [Column(TypeName = "bigint")]
    public long IssuingVolumes { get; set; }

    [Column(TypeName = "nvarchar(64)")]
    public string? IssuerId { get; set; } = string.Empty;

    
}