using System.IO.MemoryMappedFiles;

namespace Ingestion;

public sealed class Reader : IDisposable
{
    public string Path { get; }
    public long FileLength { get; }

    private readonly MemoryMappedFile? _mapping;
    private readonly MemoryMappedViewAccessor? _view;
    private bool _disposed;

    public delegate void Consumer(ReadOnlySpan<byte> bytes);

    public Reader(string path)
    {
        var fileInfo = new FileInfo(path);
        this.Path = fileInfo.FullName;
        this.FileLength = fileInfo.Length;

        if (this.FileLength > 0)
        {
            this._mapping = MemoryMappedFile.CreateFromFile(
                path,
                FileMode.Open,
                null,
                0,
                MemoryMappedFileAccess.Read
            );

            this._view = this._mapping!.CreateViewAccessor(
                0,
                0,
                MemoryMappedFileAccess.Read
            );
        }
    }

    public void Read(long offset, int length, Reader.Consumer consumer)
    {
        ObjectDisposedException.ThrowIf(this._disposed, this);

        if (offset < 0 || offset > this.FileLength)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0 || offset == this.FileLength || this._mapping is null)
        {
            consumer(ReadOnlySpan<byte>.Empty);
            return;
        }

        long availableLength = Math.Min((int)(this.FileLength - offset), length);

        var handle = this._view!.SafeMemoryMappedViewHandle;

        unsafe
        {
            byte* pointer = null;

            try
            {
                handle.AcquirePointer(ref pointer);
                pointer += this._view!.PointerOffset + offset;

                consumer(new ReadOnlySpan<byte>(pointer, (int)availableLength));
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
