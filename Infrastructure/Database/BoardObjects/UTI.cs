namespace Infrastructure.Database.BoardObjects;

using System;

public readonly struct UTI : IEquatable<UTI>
{
    private const string LEI = "253400S9DKA7JG1JWC41";
    private const string Reserved = "00";
    private const char Separator = '-';
    private static readonly string Prefix = LEI.Substring(6, 10);

    private static readonly char[] CharList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public string DocType { get; }
    public Guid Uid { get; }

    public UTI(string docType, Guid uid)
    {
        if (docType is null || docType.Length != 2)
            throw new ArgumentException("DocType must be exactly 2 characters", nameof(docType));

        DocType = docType.ToUpperInvariant();
        Uid = uid;
    }

    /// <summary>Создать новый UTI</summary>
    public static UTI New(string docType) => new(docType, Guid.NewGuid());

    /// <summary>Быстрое преобразование в строку (42 символа)</summary>
    public override string ToString()
    {
        return string.Create(42, (DocType, Uid), static (span, state) =>
        {
            var (docType, uid) = state;

            // Prefix
            Prefix.AsSpan().CopyTo(span);
            int pos = 10;
            span[pos++] = Separator;

            // DocType
            span[pos++] = docType[0];
            span[pos++] = docType[1];

            // Reserved
            span[pos++] = '0';
            span[pos++] = '0';

            // Separator
            span[pos++] = Separator;

            // Encode Base36 Guid (26 chars)
            Span<byte> bytes = stackalloc byte[16];
            uid.TryWriteBytes(bytes);

            EncodeBase36(BitConverter.ToUInt64(bytes), span.Slice(pos, 13));
            EncodeBase36(BitConverter.ToUInt64(bytes[8..]), span.Slice(pos + 13, 13));
        });
    }

    /// <summary>Попробовать разобрать UTI</summary>
    public static bool TryParse(ReadOnlySpan<char> span, out UTI result)
    {
        result = default;
        if (span.Length != 42) return false;

        // Prefix
        for (int i = 0; i < 10; i++)
            if (span[i] != Prefix[i]) return false;
        if (span[10] != Separator) return false;

        char d1 = span[11], d2 = span[12];
        if (!IsBase36Char(d1) || !IsBase36Char(d2)) return false;

        if (span[13] != '0' || span[14] != '0') return false;
        if (span[15] != Separator) return false;

        for (int i = 16; i < 42; i++)
            if (!IsBase36Char(span[i])) return false;

        var uid = DecodeBase36Guid(span.Slice(16, 26));
        result = new UTI(new string(new[] { d1, d2 }), uid);
        return true;
    }

    public static UTI Parse(string s) =>
        TryParse(s.AsSpan(), out var r) ? r : throw new FormatException("Invalid UTI");

    public static bool IsValid(string s) => TryParse(s.AsSpan(), out _);

    public bool Equals(UTI other) => DocType == other.DocType && Uid == other.Uid;
    public override bool Equals(object? obj) => obj is UTI other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(DocType, Uid);
    public static bool operator ==(UTI l, UTI r) => l.Equals(r);
    public static bool operator !=(UTI l, UTI r) => !l.Equals(r);

    #region Base36

    private static bool IsBase36Char(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z');

    private static void EncodeBase36(ulong value, Span<char> dest)
    {
        int pos = 13;
        while (value != 0 && pos > 0)
        {
            dest[--pos] = CharList[value % 36];
            value /= 36;
        }
        while (pos > 0) dest[--pos] = '0';
    }

    private static Guid DecodeBase36Guid(ReadOnlySpan<char> span)
    {
        ulong part1 = DecodeBase36(span[..13]);
        ulong part2 = DecodeBase36(span[13..]);
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.TryWriteBytes(bytes, part1);
        BitConverter.TryWriteBytes(bytes[8..], part2);
        return new Guid(bytes);
    }

    private static ulong DecodeBase36(ReadOnlySpan<char> span)
    {
        ulong result = 0;
        foreach (char c in span)
        {
            int val = c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => c - 'A' + 10,
                _ => -1
            };
            if (val < 0) throw new FormatException($"Invalid base36 char: {c}");
            result = result * 36 + (ulong)val;
        }
        return result;
    }

    #endregion
}
