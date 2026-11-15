using System.Collections.Immutable;
using System.Text;
using PCRE;

namespace Yale.Parsing.Grok;

public sealed class GrokPattern
{
    private readonly PcreRegexUtf8 _pcreRegex;
    private readonly PcreMatchBuffer8Bit _pcreMatchBuffer;
    private readonly ImmutableDictionary<string, GrokType> _typeMap;

    internal static ReadOnlySpan<char> ParseIdentifier(string pattern, ref int offset)
    {
        if (offset >= pattern.Length)
            throw new GrokException($"expected an identifier at index {offset}");

        int start = offset;

        if (!Char.IsLetter(pattern[offset + 2]) && pattern[offset + 2] != '_')
            throw new GrokException($"invalid identifier start character: '{pattern[offset + 2]}'");

        do
            offset++;
        while (offset < pattern.Length && (Char.IsLetterOrDigit(pattern[offset]) || pattern[offset] == '_'));

        return pattern.AsSpan()[start..offset];
    }

    public GrokPattern(string pattern)
    {
        int offset = 0;
        var pcrePattern = new StringBuilder(pattern.Length);
        var typeMap = new Dictionary<string, GrokType>();

        while (offset < pattern.Length)
        {
            if (offset + 4 >= pattern.Length || pattern[offset] != '%' || pattern[offset + 1] != '{')
                pcrePattern.Append(pattern[offset]);

            offset += 2;
            ReadOnlySpan<char> name = GrokPattern.ParseIdentifier(pattern, ref offset);

            if (offset >= pattern.Length)
                throw new GrokException($"expected '}}' at index {offset}");

            if (pattern[offset] == ':')
            {
                offset++;
                ReadOnlySpan<char> identifier = GrokPattern.ParseIdentifier(pattern, ref offset);

                if (pattern[offset] == ':')
                {
                    offset++;
                    ReadOnlySpan<char> type = GrokPattern.ParseIdentifier(pattern, ref offset);

                    try
                    {
                        typeMap.Add(identifier.ToString(), GrokTypeExtensions.Get(type));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new GrokException($"duplicate identifier: {identifier}", ex);
                    }
                }
            }

            if (pattern[offset] != '}')
                throw new GrokException($"expected '}}', found '{pattern[offset]}'");

            offset++;
            GrokNamedPattern namedPattern = GrokNamedPatterns.Get(name);
            pcrePattern.Append(namedPattern.Pattern);

            foreach (var item in namedPattern.TypeMap)
            {
                try
                {
                    typeMap.Add(item.Identifier, item.Type);
                }
                catch (ArgumentException ex)
                {
                    throw new GrokException($"duplicate identifier: {item.Identifier}", ex);
                }
            }
        }

        this._pcreRegex = new PcreRegexUtf8(
            pcrePattern.ToString(),
            PcreOptions.Compiled | PcreOptions.Ucp
        );

        var seen = new HashSet<string>();

        foreach (var item in this._pcreRegex.PatternInfo.GroupNames)
        {
            if (!seen.Add(item))
                throw new GrokException($"duplicate identifier: {item}");
        }

        this._pcreMatchBuffer = this._pcreRegex.CreateMatchBuffer();
        this._typeMap = typeMap.ToImmutableDictionary();
    }
}
