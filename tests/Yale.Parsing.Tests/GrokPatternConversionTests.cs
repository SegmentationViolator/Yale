using System.Globalization;

using Yale.Parsing.Grok;
using static Yale.Parsing.Tests.GrokTestHelpers;

namespace Yale.Parsing.Tests;

// GrokType is internal; verify its behavior through the public GrokPattern API.
public sealed partial class GrokPatternTests
{
    [Fact]
    public void Converts_each_supported_type_and_keeps_its_union_variant()
    {
        var pattern = new GrokPattern(@"^%{INT:a:int} %{INT:b:long} %{NUMBER:c:double} %{NUMBER:d:float} %{WORD:e:boolean} %{WORD:f}$");
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var (fields, _) = Assert.Single(pattern.Parse(Utf8("-12 2147483648 2.5 1.5 TrUe text")));
            HasValue(fields, "a", -12);
            HasValue(fields, "b", 2147483648L);
            HasValue(fields, "c", 2.5d);
            HasValue(fields, "d", 1.5f);
            HasValue(fields, "e", true);
            HasValue(fields, "f", "text");
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("FALSE", false)]
    public void Parses_booleans_with_ascii_case_and_whitespace(string input, bool expected)
    {
        var pattern = new GrokPattern(@"(?s)^%{GREEDYDATA:value:boolean}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8(input)));
        HasValue(fields, "value", expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("yes")]
    [InlineData("truex")]
    public void Invalid_boolean_is_rejected(string input)
    {
        var pattern = new GrokPattern(@"^%{GREEDYDATA:value:boolean}$");
        Assert.Throws<FormatException>(() => pattern.Parse(Utf8(input)));
    }

    [Theory]
    [InlineData("-2147483648", int.MinValue)]
    [InlineData("2147483647", int.MaxValue)]
    public void Int_boundaries_are_accepted(string input, int expected)
    {
        var pattern = new GrokPattern(@"^%{INT:value:int}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8(input)));
        HasValue(fields, "value", expected);
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    public void Int_overflow_is_rejected(string input)
    {
        var pattern = new GrokPattern(@"^%{INT:value:int}$");
        Assert.Throws<OverflowException>(() => pattern.Parse(Utf8(input)));
    }

    [Fact]
    public void Invalid_numeric_capture_is_rejected()
    {
        var pattern = new GrokPattern(@"^%{WORD:value:int}$");
        Assert.Throws<FormatException>(() => pattern.Parse(Utf8("hello")));
    }
}
