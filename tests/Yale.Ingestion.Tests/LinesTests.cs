namespace Yale.Ingestion.Tests;

public sealed class LinesTests
{
    [Fact]
    public void Stores_references_in_insertion_order()
    {
        var lines = new Lines();
        Assert.Equal(0, lines.Count);
        lines.Add(new LineReference(10, 3));
        lines.Add(new LineReference(13, 8));
        Assert.Equal(2, lines.Count);
        Assert.Equal(10, lines[0].Offset);
        Assert.Equal(3, lines[0].Length);
        Assert.Equal(13, lines[1].Offset);
        Assert.Equal(8, lines[1].Length);
    }

    [Fact]
    public void Indexer_rejects_out_of_range_indexes()
    {
        var lines = new Lines();
        Assert.Throws<ArgumentOutOfRangeException>(() => lines[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => lines[0]);
    }
}
