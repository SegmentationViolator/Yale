using System.Text;

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
    public static GrokType Get(ReadOnlySpan<char> type) => type switch
    {
        "int" => GrokType.Int,
        "long" => GrokType.Long,
        "double" => GrokType.Double,
        "float" => GrokType.Float,
        "boolean" => GrokType.Boolean,
        _ => throw new GrokException($"unknown Grok type: {type.ToString()}")
    };

    public static GrokValue Parse(this GrokType type, ReadOnlySpan<byte> subject) => type switch
    {
        GrokType.Int => int.Parse(subject),
        GrokType.Long => long.Parse(subject),
        GrokType.Double => double.Parse(subject),
        GrokType.Float => float.Parse(subject),
        GrokType.Boolean => GrokTypeExtensions.ParseBool(subject),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private static bool ParseBool(ReadOnlySpan<byte> subject)
    {
        static bool IsWhitespace(byte b) =>
            b is 0x20 or (>= 0x09 and <= 0x0D);

        while (!subject.IsEmpty && IsWhitespace(subject[0]))
            subject = subject[1..];

        while (!subject.IsEmpty && IsWhitespace(subject[^1]))
            subject = subject[..^1];

        if (Ascii.EqualsIgnoreCase(subject, "true"u8))
        {
            return true;
        }

        if (Ascii.EqualsIgnoreCase(subject, "false"u8))
        {
            return false;
        }

        throw new FormatException($"invalid bool value: '{Encoding.UTF8.GetString(subject)}'");
    }
}
