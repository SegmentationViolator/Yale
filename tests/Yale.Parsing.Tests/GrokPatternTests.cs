using Yale.Parsing.Grok;
using static Yale.Parsing.Tests.GrokTestHelpers;

namespace Yale.Parsing.Tests;

public sealed partial class GrokPatternTests
{
    [Fact]
    public void Matches_named_unnamed_and_plain_regex_parts()
    {
        var pattern = new GrokPattern(@"^%{WORD:user}-%{INT}-%{WORD:action}$");
        var (fields, range) = Assert.Single(pattern.Parse(Utf8("alice-42-start")));
        Assert.Equal(2, fields.Count);
        HasValue(fields, "user", "alice");
        HasValue(fields, "action", "start");
        Assert.Equal(0, range.Start.Value);
        Assert.Equal(14, range.End.Value);
    }

    [Fact]
    public void Finds_multiple_matches_with_byte_relative_ranges()
    {
        var pattern = new GrokPattern(@"%{WORD:word}");
        var matches = pattern.Parse(Utf8("!é!abc!"));
        Assert.Equal(2, matches.Count);
        HasValue(matches[0].Item1, "word", "é");
        HasValue(matches[1].Item1, "word", "abc");
        Assert.Equal(1, matches[0].Item2.Start.Value);
        Assert.Equal(3, matches[0].Item2.End.Value);
        Assert.Equal(4, matches[1].Item2.Start.Value);
        Assert.Equal(7, matches[1].Item2.End.Value);
    }

    [Fact]
    public void Omits_unmatched_optional_fields_but_keeps_empty_successful_captures()
    {
        var optional = new GrokPattern(@"^%{WORD:word}(?: %{INT:count:int})?$");
        var (fields, _) = Assert.Single(optional.Parse(Utf8("hello")));
        HasValue(fields, "word", "hello");
        Assert.False(fields.ContainsKey("count"));
        var empty = new GrokPattern(@"^%{DATA:value}$");
        var (emptyFields, _) = Assert.Single(empty.Parse([]));
        HasValue(emptyFields, "value", "");
    }

    [Fact]
    public void Returns_no_results_for_an_unmatched_subject()
    {
        var pattern = new GrokPattern(@"^%{INT:number:int}$");
        Assert.Empty(pattern.Parse(Utf8("no")));
    }

    [Fact]
    public void Supports_repeated_sequential_parses_without_mutating_previous_results()
    {
        var pattern = new GrokPattern(@"^%{WORD:word}$");
        var first = Assert.Single(pattern.Parse(Utf8("first")));
        var second = Assert.Single(pattern.Parse(Utf8("second")));
        HasValue(first.Item1, "word", "first");
        HasValue(second.Item1, "word", "second");
    }

    [Fact]
    public void An_unnamed_expansion_does_not_leak_alternation_outside_its_group()
    {
        var pattern = new GrokPattern(@"^%{DATE}$");
        Assert.Empty(pattern.Parse(Utf8("01/02/2024suffix")));
        Assert.Empty(pattern.Parse(Utf8("prefix31.12.2024")));
    }

    [Fact]
    public void Literal_grok_marker_can_be_expressed_as_regex()
    {
        var pattern = new GrokPattern(@"^%\{WORD\}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8("%{WORD}")));
        Assert.Empty(fields);
    }

    [Theory]
    [InlineData("%{")]
    [InlineData("%{WORD")]
    [InlineData("%{WORD:")]
    [InlineData("%{WORD:field:")]
    [InlineData("%{!}")]
    [InlineData("%{1")]
    public void Rejects_malformed_grok_patterns(string input)
    {
        Assert.Throws<GrokException>(() => new GrokPattern(input));
    }

    [Theory]
    [InlineData("%{DOESNOTEXIST:field}")]
    [InlineData("%{WORD:field:unknown}")]
    [InlineData("%{INT:value:int} %{INT:value:int}")]
    [InlineData("%{WORD:value} %{WORD:value}")]
    public void Rejects_unknown_patterns_types_and_duplicate_identifiers(string input)
    {
        Assert.Throws<GrokException>(() => new GrokPattern(input));
    }
}
