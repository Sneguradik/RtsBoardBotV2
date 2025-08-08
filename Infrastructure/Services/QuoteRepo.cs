using System.Data;
using System.Globalization;
using System.Text;
using Dapper;
using Domain.Config;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Database;
using Infrastructure.Database.BoardObjects;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class QuoteRepo( IDbConnection connection, IInstrumentRepo instrumentRepo, IOptions<BotConfig> conf) : IQuoteRepo
{
    public async Task<IEnumerable<Quote>> GetQuotesAsync(IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        
        var sql = $@"
        SELECT 
            q.Id AS DocumentId,
            q.CurrentRevisionId AS RevisionId,
            CAST(r.Price AS float) AS Price,
            CAST(r.Quantity AS float) AS Quantity,
            r.Direction,
            q.InstrumentId AS Ticker
        FROM Quotes q
        INNER JOIN QuoteRevisions r ON q.CurrentRevisionId = r.Id
        WHERE q.Id IN ({BuildInClause(ids)})";

        var rawQuotes = await connection.QueryAsync(
            new CommandDefinition(sql, cancellationToken: cancellationToken)
        ); 

        var quotes = new List<Quote>();
        foreach (var row in rawQuotes)
        {
            var instrument = instrumentRepo.GetInstrumentByTicker(row.Ticker);
            if (instrument is null) continue;

            quotes.Add(new Quote
            {
                DocumentId = row.DocumentId,
                RevisionId = row.RevisionId,
                Price = row.Price,
                Quantity = row.Quantity,
                Direction = (DealDirection)row.Direction,
                Instrument = instrument
            });
        }

        return quotes;
    }


    public async Task<Quote?> GetQuoteAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public async Task<IEnumerable<Quote>> CreateQuotesAsync(IEnumerable<string> tickers, CancellationToken cancellationToken = default)
    {
        var tickersArray = tickers.ToArray();
        var inClause = string.Join(",", tickersArray.Select((_, i) => $"'{tickersArray[i]}'"));
        var sql = $"SELECT * FROM Equities WHERE Id IN ({inClause})";

        var equities = await connection.QueryAsync<BoardEquity>(
            new CommandDefinition(sql, cancellationToken: cancellationToken)
        );

        var boardQuotes = new List<BoardQuote>();
        foreach (var equity in equities)
        {
            for (int i = 0; i < conf.Value.OrderBookDepth; i++)
            {
                boardQuotes.Add(CreateEquityQuoteAsync(equity, DealDirection.Buy));
                boardQuotes.Add(CreateEquityQuoteAsync(equity, DealDirection.Sell));
            }
        }
        
        var ids = await BulkInsertQuotesAsync(boardQuotes, cancellationToken);
        
        return await GetQuotesAsync(ids, cancellationToken);
    }

    public async Task UpdateQuotesAsync(IEnumerable<Quote> quotes, CancellationToken ct = default)
    {
        var quoteList = quotes.ToList();
        if (quoteList.Count == 0) return;

        var now = DateTime.UtcNow;
        var settlement = now.AddDays(1);

        var sb = new StringBuilder();

        foreach (var q in quoteList)
        {
            
            var descRus = $"{(q.Direction == DealDirection.Sell ? "Продажа" : "Покупка")} {q.Price} / {q.Quantity}";
            var descEng = $"{(q.Direction == DealDirection.Sell ? "Sell" : "Buy")} {q.Price} / {q.Quantity}";

            sb.AppendLine($@"
            UPDATE QuoteRevisions
            SET Price = {q.Price.ToString(CultureInfo.InvariantCulture)},
                Quantity = {q.Quantity.ToString(CultureInfo.InvariantCulture)},
                NominalValue = Price * Quantity,
                  NominalValueRub = Price * Quantity * (CASE WHEN ExchangeRate<>0 THEN ExchangeRate ELSE 1 END),
                DescriptionRus = N'{descRus.Replace("'", "''")}',
                DescriptionEng = N'{descEng.Replace("'", "''")}',
                SettlementDate = CAST(N'{settlement:yyyy-MM-dd HH:mm:ss}' AS DATETIME),
                DeliveryDate = CAST(N'{settlement:yyyy-MM-dd HH:mm:ss}' AS DATETIME),
                CreationTime = CAST(N'{now:yyyy-MM-dd HH:mm:ss}' AS DATETIME)
            WHERE Id = {q.RevisionId};

            UPDATE Quotes
            SET CreationTime = CAST(N'{now:yyyy-MM-dd HH:mm:ss}' AS DATETIME)
            WHERE Id = '{q.DocumentId}';
            ");
        }

        await connection.ExecuteAsync(new CommandDefinition(sb.ToString(), cancellationToken: ct));
    }


    public async Task DeleteQuoteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        
        var sql = $"DELETE FROM Quotes WHERE Id IN ({BuildInClause(ids)})";

        await connection.ExecuteAsync(
            new CommandDefinition(sql, cancellationToken: cancellationToken)
        );
    }



    private BoardQuote CreateEquityQuoteAsync(BoardEquity equity, DealDirection direction)
    {
        var xml = new BoardXml()
        {
            Body = GenerateXml(equity, direction)
        };
        var revision = new BoardQuoteRevision()
        {
            CounterpartyId = null,
            Comment = $"Quote for {equity.Id}",
            ClientCode = "проверка123",
            IsIndicative = false,
            Price = 1,
            Quantity = 1,
            PriceCurrencyId = equity.CurrencyId,
            NominalCurrencyId = equity.CurrencyId,
            TimeToLive = 0,
            ExpirationDate = null,
            DeliveryMethod = "DeliveryVersusPayment",
            SettlementDate = DateTime.UtcNow+TimeSpan.FromDays(1),
            DeliveryDate = DateTime.UtcNow+TimeSpan.FromDays(1),
            SettlementCurrencyId = equity.CurrencyId,
            ExchangeRate = 0,
            Number = 2,
            CreatedById = 1562,
            State = "ACTIVE",
            ErrorCode = null,
            ErrorTextRus = null,
            ErrorTextEng = null,
            CreationTime = DateTime.UtcNow,
            Direction = direction==DealDirection.Buy?1:-1,
            IsValid = true,
            StandardPrice = null,
            ProductSpecificParams = null,
            IsPartialExecution = true,
            IsInformationQuote = false,
            IsAnonymousQuote = false,
            ShowIfTheBest = false,
            SettlementPlace = "NSD",
            Xml = xml
        };
        var quote = new BoardQuote()
        {
            Id = UTI.New("QT").ToString(),
            CreatedById = 1562,
            CreationTime = DateTime.UtcNow,
            FrontTradeId = null,
            InstrumentId = equity.Id,
            IsDynamic = false,
            PartyId = "AUTO_Q",
            QuoteRequestId = null,
            QuoteReplyId = null,
            LockOwnerId = null,
            LockTime = null,
            CurrentRevision = revision,
        };
        return quote;
    }

    public async Task<IEnumerable<string>> BulkInsertQuotesAsync(IEnumerable<BoardQuote> quotes, CancellationToken ct = default)
    {

        var list = quotes.ToList();
        if (list.Count == 0) return [];

        using var tran = connection.BeginTransaction();

        // 1️⃣ Вставка Quotes
        var quoteInsert = @"
            INSERT INTO Quotes (
                Id, CreatedById, CreationTime, CurrentRevisionId,
                FrontTradeId, InstrumentId, IsDynamic,
                LockOwnerId, LockTime, PartyId, QuoteReplyId, QuoteRequestId
            ) VALUES
        ";

        quoteInsert += string.Join(',', list.Select(q =>
            $"('{q.Id}', {q.CreatedById}, CAST(N'{q.CreationTime:yyyy-MM-dd HH:mm:ss}' AS DATETIME), NULL, " +
            $"{(q.FrontTradeId == null ? "NULL" : $"'{q.FrontTradeId}'")}, " +
            $"'{q.InstrumentId}', {Convert.ToInt16(q.IsDynamic)}, " +
            $"{(q.LockOwnerId.HasValue ? q.LockOwnerId.Value : "NULL")}, " +
            $"{(q.LockTime.HasValue ? $"CAST(N'{q.LockTime:yyyy-MM-dd HH:mm:ss}' AS DATETIME)" : "NULL")}, " +
            $"'{q.PartyId}', " +
            $"{(q.QuoteReplyId == null ? "NULL" : $"'{q.QuoteReplyId}'")}, " +
            $"{(q.QuoteRequestId == null ? "NULL" : $"'{q.QuoteRequestId}'")})"
        ));

        await connection.ExecuteAsync(new CommandDefinition(quoteInsert, transaction: tran, cancellationToken: ct));


        // 2️⃣ Вставка Xml и обновление Id в сущностях
        var xmlInsert = "INSERT INTO Xml(Body) OUTPUT INSERTED.Id VALUES " +
                        string.Join(',', list.Select(q =>
                            $"(N'{q.CurrentRevision.Xml.Body.Replace("'", "''")}')"));

        var xmlIds = (await connection.QueryAsync<int>(
            new CommandDefinition(xmlInsert, transaction: tran, cancellationToken: ct)
        )).ToList();

        for (int i = 0; i < list.Count; i++)
            list[i].CurrentRevision.Xml.Id = xmlIds[i];


        // 3️⃣ Вставка QuoteRevisions и обновление Id в сущностях
        var revisionInsert = @"
            INSERT INTO QuoteRevisions (
                IsAnonymousQuote, SettlementPlace, ShowIfTheBest,
                CounterpartyId, Comment, ClientCode, IsIndicative,
                Price, Quantity, PriceCurrencyId, NominalValue, NominalValueRub,
                NominalCurrencyId, TimeToLive,
                ExpirationDate, DeliveryMethod, SettlementDate, DeliveryDate, SettlementCurrencyId,
                ExchangeRate, Number, CreatedById, State, DocumentId,
                DescriptionRus, DescriptionEng,
                ErrorCode, ErrorTextRus, ErrorTextEng, XmlId, CreationTime, Direction, IsValid,
                StandardPrice, ProductSpecificParams, IsPartialExecution, IsInformationQuote
            )
            OUTPUT INSERTED.Id
            VALUES
        ";

        revisionInsert += string.Join(',', list.Select((q, i) =>
        {
            var r = q.CurrentRevision;

            return $"({Convert.ToInt16(r.IsAnonymousQuote)}, '{r.SettlementPlace}', {Convert.ToInt16(r.ShowIfTheBest)}, " +
                   $"{(r.CounterpartyId == null ? "NULL" : $"'{r.CounterpartyId}'")}, " +
                   $"'{r.Comment.Replace("'", "''")}', '{r.ClientCode}', {Convert.ToInt16(r.IsIndicative)}, " +
                   $"{r.Price.ToString(CultureInfo.InvariantCulture)}, {r.Quantity.ToString(CultureInfo.InvariantCulture)}, '{r.PriceCurrencyId}', " +
                   $"{r.NominalValue.ToString(CultureInfo.InvariantCulture)}, {r.NominalValueRub.ToString(CultureInfo.InvariantCulture)}, " +
                   $"'{r.NominalCurrencyId}', {r.TimeToLive}, " +
                   $"{(r.ExpirationDate.HasValue ? $"CAST(N'{r.ExpirationDate:yyyy-MM-dd HH:mm:ss}' AS DATETIME)" : "NULL")}, " +
                   $"'{r.DeliveryMethod}', " +
                   $"CAST(N'{r.SettlementDate:yyyy-MM-dd HH:mm:ss}' AS DATETIME), " +
                   $"CAST(N'{r.DeliveryDate:yyyy-MM-dd HH:mm:ss}' AS DATETIME), '{r.SettlementCurrencyId}', " +
                   $"{r.ExchangeRate.ToString(CultureInfo.InvariantCulture)}, {r.Number}, {r.CreatedById}, '{r.State}', '{q.Id}', " +
                   $"N'{r.DescriptionRus.Replace("'", "''")}', N'{r.DescriptionEng.Replace("'", "''")}', " +
                   $"{(r.ErrorCode == null ? "NULL" : $"'{r.ErrorCode}'")}, " +
                   $"{(r.ErrorTextRus == null ? "NULL" : $"N'{r.ErrorTextRus.Replace("'", "''")}'")}, " +
                   $"{(r.ErrorTextEng == null ? "NULL" : $"N'{r.ErrorTextEng.Replace("'", "''")}'")}, " +
                   $"{xmlIds[i]}, " +
                   $"CAST(N'{r.DeliveryDate:yyyy-MM-dd HH:mm:ss}' AS DATETIME), {r.Direction}, {Convert.ToInt16(r.IsValid == true)}, " +
                   $"{(r.StandardPrice.HasValue ? r.StandardPrice.Value.ToString(CultureInfo.InvariantCulture) : "NULL")}, " +
                   $"{(r.ProductSpecificParams == null ? "NULL" : $"N'{r.ProductSpecificParams.Replace("'", "''")}'")}, " +
                   $"{Convert.ToInt16(r.IsPartialExecution)}, {Convert.ToInt16(r.IsInformationQuote)})";
        }));
        var revisionIds = (await connection.QueryAsync<int>(
            new CommandDefinition(revisionInsert, transaction: tran, cancellationToken: ct)
        )).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            list[i].CurrentRevision.Id = revisionIds[i];
            list[i].CurrentRevisionId = revisionIds[i];
        }


        // 4️⃣ Обновление CurrentRevisionId в Quotes
        await connection.ExecuteAsync(@"
            UPDATE q
            SET CurrentRevisionId = r.Id
            FROM Quotes q
            JOIN QuoteRevisions r ON q.Id = r.DocumentId
            WHERE q.CurrentRevisionId IS NULL;
        ", transaction: tran);

        tran.Commit();
        return list.Select(q => q.Id);


    }
    
    private static string  BuildInClause(IEnumerable<string> values)
    {
        return string.Join(",", values.Select(id => $"'{id}'"));
    }


    private static string GenerateXml(BoardEquity equity, DealDirection direction) =>
        $"<rtsotc:equityTransaction id=\"EquityTransaction1\" xmlns=\"http://www.fpml.org/FpML-5/confirmation\" xmlns:rtsotc=\"http://www.fpml.ru/otc-system\" xmlns:rtsrep=\"http://www.fpml.ru/repository\" xmlns:fpmlext=\"http://www.fpml.org/FpML-5/ext\" xmlns:dsig=\"http://www.w3.org/2000/09/xmldsig#\"><productType>SecurityTransaction</productType><productId>{equity.Id}</productId><buyerPartyReference href=\"{(direction == DealDirection.Buy ? "quote-owner" : "counterparty")}\"/><sellerPartyReference href=\"{(direction == DealDirection.Buy ? "counterparty" : "quote-owner")}\"/><rtsotc:issuingVolumes>{equity.IssuingVolumes}</rtsotc:issuingVolumes><rtsotc:numberOfUnits>1.0000</rtsotc:numberOfUnits><rtsotc:unitPrice>1.0000</rtsotc:unitPrice><rtsotc:priceCurrency>{equity.CurrencyId}</rtsotc:priceCurrency><rtsotc:equity id=\"{equity.Id}\"><instrumentId instrumentIdScheme=\"http://www.fpml.ru/coding-scheme/instrument-id#code\">{equity.Id}</instrumentId><instrumentId instrumentIdScheme=\"http://www.fpml.ru/coding-scheme/instrument-id#regnum\">1-02-12500-A</instrumentId><instrumentId instrumentIdScheme=\"http://www.fpml.ru/coding-scheme/instrument-id#isin\">{equity.ISIN}</instrumentId><description>{equity.IssuerRus}</description><currency id=\"Currency1\">{equity.CurrencyId}</currency></rtsotc:equity><rtsotc:unitNotional>1.00000</rtsotc:unitNotional><rtsotc:deliveryMethod>DeliveryVersusPayment</rtsotc:deliveryMethod><rtsotc:settlementDate>{DateTime.UtcNow:yyyy-MM-dd}</rtsotc:settlementDate><rtsotc:deliveryDate>{DateTime.UtcNow:yyyy-MM-dd}</rtsotc:deliveryDate><rtsotc:settlementCurrency>{equity.CurrencyId}</rtsotc:settlementCurrency></rtsotc:equityTransaction>";
}