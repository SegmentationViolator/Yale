namespace Yale.Ingestion.Tests;

internal sealed class TestFile : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"yale-{Guid.NewGuid():N}");

    public TestFile(byte[] contents) => File.WriteAllBytes(Path, contents);

    public void Dispose() => File.Delete(Path);
}
