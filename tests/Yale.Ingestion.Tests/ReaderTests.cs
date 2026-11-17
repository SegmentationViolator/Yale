using System.Text;

namespace Yale.Ingestion.Tests;

public sealed class ReaderTests
{
    private static byte[] Read(Reader reader, long offset, int length)
    {
        byte[]? result = null;
        reader.Read(offset, length, bytes => result = bytes.ToArray());
        Assert.NotNull(result);
        return result;
    }

    [Fact]
    public void Exposes_full_path_and_byte_length()
    {
        using var file = new TestFile(Encoding.UTF8.GetBytes("aé"));
        using var reader = new Reader(file.Path);
        Assert.Equal(System.IO.Path.GetFullPath(file.Path), reader.Path);
        Assert.Equal(3, reader.FileLength);
    }

    [Theory]
    [InlineData(0, 2, "ab")]
    [InlineData(2, 2, "cd")]
    [InlineData(4, 20, "ef")]
    [InlineData(6, 1, "")]
    [InlineData(1, 0, "")]
    public void Reads_requested_range_and_clamps_at_end(long offset, int length, string expected)
    {
        using var file = new TestFile("abcdef"u8.ToArray());
        using var reader = new Reader(file.Path);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), Read(reader, offset, length));
    }

    [Fact]
    public void Empty_file_is_readable()
    {
        using var file = new TestFile([]);
        using var reader = new Reader(file.Path);
        Assert.Equal(0, reader.FileLength);
        Assert.Empty(Read(reader, 0, 5));
    }

    [Fact]
    public void Read_invokes_action_once_even_for_empty_results()
    {
        using var file = new TestFile([]);
        using var reader = new Reader(file.Path);
        int calls = 0;
        reader.Read(0, 1, bytes => { calls++; Assert.True(bytes.IsEmpty); });
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Rejects_negative_and_out_of_bounds_arguments()
    {
        using var file = new TestFile("abc"u8.ToArray());
        using var reader = new Reader(file.Path);
        Assert.Equal("offset", Assert.Throws<ArgumentOutOfRangeException>(() => reader.Read(-1, 1, _ => { })).ParamName);
        Assert.Equal("offset", Assert.Throws<ArgumentOutOfRangeException>(() => reader.Read(4, 1, _ => { })).ParamName);
        Assert.Equal("length", Assert.Throws<ArgumentOutOfRangeException>(() => reader.Read(0, -1, _ => { })).ParamName);
    }

    [Fact]
    public void Can_read_again_after_callback_throws()
    {
        using var file = new TestFile("abc"u8.ToArray());
        using var reader = new Reader(file.Path);
        Assert.Throws<InvalidOperationException>(() => reader.Read(0, 1, _ => throw new InvalidOperationException()));
        Assert.Equal("abc"u8.ToArray(), Read(reader, 0, 3));
    }

    [Fact]
    public void Dispose_is_idempotent_and_read_after_dispose_throws()
    {
        using var file = new TestFile("abc"u8.ToArray());
        var reader = new Reader(file.Path);
        reader.Dispose();
        reader.Dispose();
        Assert.Throws<ObjectDisposedException>(() => reader.Read(0, 1, _ => { }));
    }

    // File.SetLength creates a sparse file on the tested Linux filesystem.
    // Avoid allocating multi-gigabyte files on Windows or 32-bit processes.
    private sealed class SparseFileFactAttribute : FactAttribute
    {
        public SparseFileFactAttribute()
        {
            if (!OperatingSystem.IsLinux() || !Environment.Is64BitProcess)
                Skip = "Requires a 64-bit Linux environment and sparse-file support.";
        }
    }

    private const long LargeFileSize = (long)int.MaxValue + 32;

    private static TestFile CreateLargeFile()
    {
        var file = new TestFile([]);
        try
        {
            using var stream = new FileStream(file.Path, FileMode.Open, FileAccess.Write, FileShare.None);
            stream.SetLength(LargeFileSize);
            stream.Position = 0;
            stream.WriteByte(0x42);
            stream.Position = LargeFileSize - 1;
            stream.WriteByte(0x43);
            return file;
        }
        catch
        {
            file.Dispose();
            throw;
        }
    }

    [SparseFileFact]
    [Trait("Category", "LargeFile")]
    public void Reads_near_the_start_of_a_file_larger_than_two_gibibytes()
    {
        using var file = CreateLargeFile();
        using var reader = new Reader(file.Path);
        Assert.Equal(new byte[] { 0x42 }, Read(reader, 0, 1));
    }

    [SparseFileFact]
    [Trait("Category", "LargeFile")]
    public void Reads_near_the_end_of_a_file_larger_than_two_gibibytes()
    {
        using var file = CreateLargeFile();
        using var reader = new Reader(file.Path);
        Assert.Equal(new byte[] { 0x43 }, Read(reader, LargeFileSize - 1, 1));
    }
}
