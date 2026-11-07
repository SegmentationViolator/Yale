namespace Ingestion;

public sealed class LineScanner
{
    private const int ChunkSize = 4 * 1024 * 1024;

    private long _scanOffset = 0;
    private long _lineStartOffset = 0;

    public int Scan(Reader reader, Lines lines, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        int oldCount = lines.Count;

        while (this._scanOffset < reader.FileLength && lines.Count - oldCount < count)
        {
            reader.Read(
                this._scanOffset,
                LineScanner.ChunkSize,
                bytes =>
                {
                    while (!bytes.IsEmpty && lines.Count - oldCount < count)
                    {
                        int newlineOffset = bytes.IndexOf((byte)'\n');

                        if (newlineOffset < 0)
                        {
                            this._scanOffset += bytes.Length;
                            break;
                        }

                        int consumed = newlineOffset + 1;
                        long lineEnd = this._scanOffset + consumed;

                        lines.Add(new LineReference(
                            this._lineStartOffset,
                            lineEnd - this._lineStartOffset
                        ));

                        this._scanOffset += consumed;
                        this._lineStartOffset = this._scanOffset;
                        bytes = bytes[consumed..];
                    }
                }
            );
        }

        if (lines.Count - oldCount < count &&
            this._scanOffset == reader.FileLength &&
            this._lineStartOffset < reader.FileLength)
        {
            lines.Add(new LineReference(
                this._lineStartOffset,
                reader.FileLength - this._lineStartOffset));

            this._lineStartOffset = reader.FileLength;
        }

        return lines.Count - oldCount;
    }
}
