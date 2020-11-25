using System.Buffers;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// The enhanced stream.
    /// </summary>
    public interface IStream2 : IDisposable
    {
        /// <summary>
        /// Get the md5 hash content.
        /// </summary>
        /// <returns>The MD5 hash string in lowercase.</returns>
        Task<string> Md5HashAsync();

        /// <summary>
        /// The stream length
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Open the stream content.
        /// </summary>
        /// <returns>The stream to use.</returns>
        Stream OpenRead();
    }

    /// <summary>
    /// The temporary file stream.
    /// </summary>
    public class TemporaryFileStream2 : IStream2
    {
        private FileInfo? _file;
        private bool _disposed;

        private TemporaryFileStream2(FileInfo fileInfo)
        {
            _file = fileInfo;
        }

        public static async Task<IStream2> CreateAsync(Stream source)
        {
            var fileName = Path.GetTempFileName();
            var fi = new FileInfo(fileName);
            using (var sw = fi.OpenWrite())
                await source.CopyToAsync(sw);
            return new TemporaryFileStream2(fi);
        }

        private T EnsureNotDisposed<T>(Func<T> valueFactory)
        {
            if (_disposed) throw new ObjectDisposedException("Temporary File Stream");
            return valueFactory.Invoke();
        }

        public long Length => EnsureNotDisposed(() => _file!.Length);

        public Task<string> Md5HashAsync() => EnsureNotDisposed(() =>
        {
            using var stream = OpenRead();
            return Task.FromResult(stream.ToMD5().ToHexDigest(true));
        });

        public Stream OpenRead() => EnsureNotDisposed(() => _file!.OpenRead());

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _file!.Delete();
                    _file = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// The temporary memory stream.
    /// </summary>
    public class RentMemoryStream2 : IStream2
    {
        private bool _disposed;
        private byte[]? _rentMemory;
        private readonly int _length;

        private RentMemoryStream2(int len)
        {
            _rentMemory = ArrayPool<byte>.Shared.Rent(len);
            _length = len;
        }

        public static async Task<IStream2> CreateAsync(Stream source)
        {
            int len = checked((int)source.Length);
            var stream = new RentMemoryStream2(len);
            for (int i = 0; i < len;)
                i += await source.ReadAsync(stream._rentMemory!, i, len - i);
            return stream;
        }

        private T EnsureNotDisposed<T>(Func<T> valueFactory)
        {
            if (_disposed) throw new ObjectDisposedException("Rent Memory Stream");
            return valueFactory.Invoke();
        }

        public long Length => EnsureNotDisposed(() => _length);

        public Task<string> Md5HashAsync() => EnsureNotDisposed(() =>
        {
            using var stream = OpenRead();
            return Task.FromResult(stream.ToMD5().ToHexDigest(true));
        });

        public Stream OpenRead() => EnsureNotDisposed(() => new MemoryStream(_rentMemory!, 0, _length, false));

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ArrayPool<byte>.Shared.Return(_rentMemory);
                    _rentMemory = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Extension for stream2.
    /// </summary>
    public static class Stream2Extensions
    {
        /// <summary>
        /// Create an <see cref="IStream2"/> instance.
        /// </summary>
        /// <param name="stream">The stream source.</param>
        /// <returns>The stream.</returns>
        public static async Task<IStream2> CreateAsync(this (Func<Stream>, long) stream)
        {
            using var s = stream.Item1.Invoke();
            if (stream.Item2 > 10 * 1024 * 1024)
                return await TemporaryFileStream2.CreateAsync(s);
            else
                return await RentMemoryStream2.CreateAsync(s);
        }
    }
}
