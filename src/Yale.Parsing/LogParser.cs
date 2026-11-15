using System.Buffers;
using System.Text;

namespace Yale.Parsing;

using Grok;

public sealed class LogParser
{
    private readonly GrokPattern _pattern;

    private static ReadOnlySpan<byte> TrimInvalidUtf8Tail(ReadOnlySpan<byte> input)
    {
        while (!input.IsEmpty)
        {
            OperationStatus status =
                Rune.DecodeLastFromUtf8(input, out _, out int bytesConsumed);

            if (status == OperationStatus.Done)
                break;

            input = input[..^bytesConsumed];
        }

        return input;
    }

    public LogParser(string pattern)
    {
        this._pattern = new GrokPattern(pattern);
    }
}
