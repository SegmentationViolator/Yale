namespace Ingestion;

public sealed class Lines
{
    private readonly List<LineReference> _lines = new(1024);

    public int Count => _lines.Count;

    public LineReference this[int index] => _lines[index];

    public void Add(LineReference line)
    {
        _lines.Add(line);
    }
}
