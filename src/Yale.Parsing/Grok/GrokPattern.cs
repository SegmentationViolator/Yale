global using GrokValue = Unio.Unio<int, long, double, float, bool, string>;
global using GrokResult = (System.Collections.Immutable.ImmutableDictionary<string, Unio.Unio<int, long, double, float, bool, string>>, System.Range);

using System.Collections.Immutable;
using System.Text;
using PCRE;

namespace Yale.Parsing.Grok;

public sealed class GrokPattern
{
    private readonly PcreRegexUtf8 _pcreRegex;
    private readonly PcreMatchBuffer8Bit _pcreMatchBuffer;
    private readonly ImmutableArray<(string, int)> _indexMap;
    private readonly ImmutableDictionary<string, GrokType> _typeMap;

    private static ReadOnlySpan<char> ParseIdentifier(string pattern, scoped ref int offset)
    {
        if (offset >= pattern.Length)
            throw new GrokException($"expected an identifier at index {offset}");

        int start = offset;

        if (!Char.IsLetter(pattern[offset]) && pattern[offset] != '_')
            throw new GrokException($"invalid identifier start character: '{pattern[offset]}'");

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
            if (offset + 1 >= pattern.Length || pattern[offset] != '%' || pattern[offset + 1] != '{')
            {
                pcrePattern.Append(pattern[offset++]);
                continue;
            }

            offset += 2;
            ReadOnlySpan<char> name = GrokPattern.ParseIdentifier(pattern, ref offset);

            var identifier = ReadOnlySpan<char>.Empty;

            if (offset < pattern.Length && pattern[offset] == ':')
            {
                offset++;
                identifier = GrokPattern.ParseIdentifier(pattern, ref offset);

                if (offset < pattern.Length && pattern[offset] == ':')
                {
                    offset++;
                    ReadOnlySpan<char> type = GrokPattern.ParseIdentifier(pattern, ref offset);

                    if (!typeMap.TryAdd(identifier.ToString(), GrokTypeExtensions.Get(type)))
                        throw new GrokException($"duplicate identifier: {identifier}");
                }
            }

            if (offset >= pattern.Length)
                throw new GrokException($"expected '}}' at index {offset}");

            if (pattern[offset] != '}')
                throw new GrokException($"expected '}}', found '{pattern[offset]}'");

            offset++;
            GrokNamedPattern namedPattern = GrokNamedPatterns.Get(name);

            if (!identifier.IsEmpty)
                pcrePattern.Append("(?<")
                    .Append(identifier)
                    .Append('>')
                    .Append(namedPattern.Pattern)
                    .Append(')');
            else
                pcrePattern.Append("(?:")
                    .Append(namedPattern.Pattern)
                    .Append(')');

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

        PcrePatternInfo patternInfo = this._pcreRegex.PatternInfo;
        IReadOnlyList<string> groupNames = patternInfo.GroupNames;
        var indexMap = new List<(string, int)>(groupNames.Count);

        foreach (var name in groupNames)
        {
            IReadOnlyList<int> indexes = patternInfo.GetGroupIndexesByName(name);

            if (indexes.Count > 1)
                throw new GrokException($"duplicate identifier: {name}");

            indexMap.Add((name, indexes[0]));
        }

        this._pcreMatchBuffer = this._pcreRegex.CreateMatchBuffer();
        this._indexMap = indexMap.ToImmutableArray();
        this._typeMap = typeMap.ToImmutableDictionary();
    }

    public List<GrokResult> Parse(ReadOnlySpan<byte> subject)
    {
        var matches = new List<GrokResult>(this._indexMap.Length);

        foreach (var match in this._pcreMatchBuffer.Matches(subject, 0, PcreMatchOptions.NoUtfCheck, null))
        {
            var entry = new Dictionary<string, GrokValue>();

            foreach (var (name, index) in this._indexMap)
            {
                if (match.TryGetGroup(index, out var group) && group.Success)
                {
                    GrokValue value;

                    if (this._typeMap.TryGetValue(name, out var type))
                        value = type.Parse(group.Value);
                    else
                        value = Encoding.UTF8.GetString(group.Value);

                    entry.Add(name, value);
                }
            }

            matches.Add((entry.ToImmutableDictionary(), match.Index..match.EndIndex));
        }

        return matches;
    }
}
