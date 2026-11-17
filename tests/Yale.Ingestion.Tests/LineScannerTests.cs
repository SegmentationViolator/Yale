using System.Text;

namespace Yale.Ingestion.Tests;

public sealed class LineScannerTests
{
    private static byte[] Read(Reader reader, LineReference line)
    {
        byte[]? result = null;
        reader.Read(line.Offset, checked((int)line.Length), bytes => result = bytes.ToArray());
        return Assert.IsType<byte[]>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("one")]
    [InlineData("one\n")]
    [InlineData("\n\n")]
    [InlineData("a\r\nb\nc\r")]
    [InlineData("é\n東京\nlast")]
    public void Scans_expected_line_slices_and_reconstructs_original_bytes(string text)
    {
        byte[] input = Encoding.UTF8.GetBytes(text);
        using var file = new TestFile(input);
        using var reader = new Reader(file.Path);
        var lines = new Lines();
        var scanner = new LineScanner();

        // Independently compute the expected newline-terminated byte slices.
        var expected = new List<byte[]>();
        int start = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == (byte)'\n')
            {
                expected.Add(input[start..(i + 1)]);
                start = i + 1;
            }
        }
        if (start < input.Length)
            expected.Add(input[start..]);

        Assert.Equal(expected.Count, scanner.Scan(reader, lines, int.MaxValue));
        Assert.Equal(expected.Count, lines.Count);
        long expectedOffset = 0;
        var reconstructed = new List<byte>();
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expectedOffset, lines[i].Offset);
            Assert.Equal(expected[i].Length, lines[i].Length);
            byte[] actual = Read(reader, lines[i]);
            Assert.Equal(expected[i], actual);
            reconstructed.AddRange(actual);
            expectedOffset += expected[i].Length;
        }
        Assert.Equal(input, reconstructed.ToArray());
        Assert.Equal(0, scanner.Scan(reader, lines, 1));
    }

    [Fact]
    public void Resumes_without_skipping_lines_and_respects_requested_count()
    {
        using var file = new TestFile("first\nsecond\nthird"u8.ToArray());
        using var reader = new Reader(file.Path);
        var lines = new Lines();
        var scanner = new LineScanner();
        Assert.Equal(0, scanner.Scan(reader, lines, 0));
        Assert.Equal(1, scanner.Scan(reader, lines, 1));
        Assert.Equal(1, scanner.Scan(reader, lines, 1));
        Assert.Equal(1, scanner.Scan(reader, lines, 1));
        Assert.Equal(0, scanner.Scan(reader, lines, 1));
        Assert.Equal(3, lines.Count);
        Assert.Equal("first\n"u8.ToArray(), Read(reader, lines[0]));
        Assert.Equal("second\n"u8.ToArray(), Read(reader, lines[1]));
        Assert.Equal("third"u8.ToArray(), Read(reader, lines[2]));
    }

    [Fact]
    public void Scans_arbitrary_bytes_without_interpreting_them_as_text()
    {
        byte[] input = [0xff, 0x00, 0x0a, 0x80, 0x0a, 0xfe];
        using var file = new TestFile(input);
        using var reader = new Reader(file.Path);
        var lines = new Lines();
        Assert.Equal(3, new LineScanner().Scan(reader, lines, 10));
        Assert.Equal(new byte[] { 0xff, 0x00, 0x0a }, Read(reader, lines[0]));
        Assert.Equal(new byte[] { 0x80, 0x0a }, Read(reader, lines[1]));
        Assert.Equal(new byte[] { 0xfe }, Read(reader, lines[2]));
    }

    [Fact]
    public void Negative_count_is_rejected_without_adding_lines()
    {
        using var file = new TestFile("abc"u8.ToArray());
        using var reader = new Reader(file.Path);
        var lines = new Lines();
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => new LineScanner().Scan(reader, lines, -1)).ParamName);
        Assert.Equal(0, lines.Count);
    }

    [Theory]
    [InlineData(4 * 1024 * 1024 - 1)]
    [InlineData(4 * 1024 * 1024)]
    [InlineData(4 * 1024 * 1024 + 1)]
    public void Finds_newlines_on_both_sides_of_the_chunk_boundary(int newlineOffset)
    {
        byte[] input = new byte[newlineOffset + 2];
        Array.Fill(input, (byte)'x');
        input[newlineOffset] = (byte)'\n';
        using var file = new TestFile(input);
        using var reader = new Reader(file.Path);
        var lines = new Lines();
        var scanner = new LineScanner();
        Assert.Equal(1, scanner.Scan(reader, lines, 1));
        Assert.Equal(1, scanner.Scan(reader, lines, 1));
        Assert.Equal(0, lines[0].Offset);
        Assert.Equal(newlineOffset + 1, lines[0].Length);
        Assert.Equal(newlineOffset + 1, lines[1].Offset);
        Assert.Equal(1, lines[1].Length);
        Assert.Equal(input, Read(reader, lines[0]).Concat(Read(reader, lines[1])).ToArray());
    }
}
