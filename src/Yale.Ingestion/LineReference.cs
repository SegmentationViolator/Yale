namespace Yale.Ingestion;

public readonly struct LineReference(long offset, long length)
{
    public long Offset { get; } = offset;
    public long Length { get; } = length;
}
