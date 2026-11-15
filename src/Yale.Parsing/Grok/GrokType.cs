using Unio;

namespace Yale.Parsing.Grok;

internal enum GrokType
{
    Int,
    Long,
    Double,
    Float,
    Boolean,
}

internal static class GrokTypeExtensions
{
    public static Unio<int, long, double, float, bool, DateTime> Parse(this GrokType type, ReadOnlySpan<char> value) => type switch
    {
        GrokType.Int => int.Parse(value),
        GrokType.Long => long.Parse(value),
        GrokType.Double => double.Parse(value),
        GrokType.Float => float.Parse(value),
        GrokType.Boolean => bool.Parse(value),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public static GrokType Get(ReadOnlySpan<char> type) => type switch
    {
        "int" => GrokType.Int,
        "long" => GrokType.Long,
        "double" => GrokType.Double,
        "float" => GrokType.Float,
        "boolean" => GrokType.Boolean,
        _ => throw new ArgumentException($"unknown Grok type: {name.ToString()}", nameof(name))
    };
}
