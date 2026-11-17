using System.Collections.Immutable;
using System.Text;
using GrokValue = Unio.Unio<int, long, double, float, bool, string>;

namespace Yale.Parsing.Tests;

internal static class GrokTestHelpers
{
    public static byte[] Utf8(string value) => Encoding.UTF8.GetBytes(value);

    public static void HasValue(ImmutableDictionary<string, GrokValue> fields, string name, GrokValue expected)
    {
        Assert.True(fields.TryGetValue(name, out var actual), $"Missing field: {name}");
        Assert.Equal(expected, actual);
    }
}
