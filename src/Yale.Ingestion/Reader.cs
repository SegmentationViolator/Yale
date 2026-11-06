using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Ingestion;

public sealed class Reader : IDisposable
{
    private const long ViewLength = 64 * 1024 * 1024;

    public string Path { get; }
    private readonly long _fileLength;
    private long _viewStart;
    private long _viewEnd;

    private readonly MemoryMappedFile? _mapping;
    private MemoryMappedViewAccessor? _view;
    private bool _disposed;

    private static int GetValidUtf8PrefixLength(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return 0;

        var status = Rune.DecodeLastFromUtf8(
            bytes,
            out _,
            out int bytesConsumed
        );

        return status switch
        {
            OperationStatus.Done => bytes.Length,
            OperationStatus.NeedMoreData => bytes.Length - bytesConsumed,
            OperationStatus.InvalidData => throw new DecoderFallbackException(),
            _ => throw new InvalidOperationException()
        };
    }

    public Reader(string path)
    {
        var fileInfo = new FileInfo(path);

        this.Path = fileInfo.FullName;
        this._fileLength = fileInfo.Length;
        this._viewStart = 0;
        this._viewEnd = 0;

        if (this._fileLength > 0)
            this._mapping = MemoryMappedFile.CreateFromFile(
                path,
                FileMode.Open,
                null,
                this._fileLength,
                MemoryMappedFileAccess.Read
            );
    }

    public string Read(long offset, long length, out int bytesUsed)
    {
        ObjectDisposedException.ThrowIf(this._disposed, this);

        bytesUsed = 0;

        if (offset < 0 || offset > this._fileLength)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (length < 0 || length > Reader.ViewLength)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0 || offset == this._fileLength || this._mapping is null)
            return string.Empty;

        long availableLength = Math.Min(this._fileLength - offset, length);

        if (this._view is null || offset < this._viewStart || offset > this._viewEnd - availableLength)
        {
            var newViewEnd = Math.Min(this._fileLength, offset + Reader.ViewLength);
            var newViewStart = Math.Max(0, newViewEnd - Reader.ViewLength);
            var newView = this._mapping!.CreateViewAccessor(
                newViewStart,
                newViewEnd - newViewStart,
                MemoryMappedFileAccess.Read
            );

            this._view?.Dispose();
            this._view = newView;
            this._viewStart = newViewStart;
            this._viewEnd = newViewEnd;
        }

        var handle = this._view!.SafeMemoryMappedViewHandle;

        unsafe
        {
            byte* pointer = null;

            try
            {
                handle.AcquirePointer(ref pointer);
                pointer += this._view.PointerOffset + (offset - this._viewStart);

                var bytes = new ReadOnlySpan<byte>(pointer, (int)availableLength);
                bytesUsed = GetValidUtf8PrefixLength(bytes);

                return Encoding.UTF8.GetString(bytes[..bytesUsed]);
            }
            finally
            {
                if (pointer != null)
                    handle.ReleasePointer();
            }
        }
    }

    public void Dispose()
    {
        if (this._disposed)
            return;

        this._disposed = true;

        this._view?.Dispose();
        this._mapping?.Dispose();
    }
}
