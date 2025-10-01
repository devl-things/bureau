namespace Niles.IO
{
    /// <summary>
    /// A read-only stream wrapper that forwards reads to an inner stream
    /// and mirrors the bytes into a second stream (e.g., MemoryStream).
    /// </summary>
    public sealed class TeeReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly Stream _mirror;

        public TeeReadStream(Stream inner, Stream mirror)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (mirror == null) throw new ArgumentNullException(nameof(mirror));
            if (!inner.CanRead) throw new ArgumentException("Inner stream must be readable.", nameof(inner));
            if (!mirror.CanWrite) throw new ArgumentException("Mirror stream must be writable.", nameof(mirror));

            _inner = inner;
            _mirror = mirror;
        }

        public override bool CanRead { get { return _inner.CanRead; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush() { /* no-op */ }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _inner.Read(buffer, offset, count);
            if (read > 0)
            {
                _mirror.Write(buffer, offset, read);
            }
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
            if (read > 0)
            {
                await _mirror.WriteAsync(buffer, offset, read, cancellationToken);
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }

        protected override void Dispose(bool disposing)
        {
            // Important: we do NOT own the mirror stream; caller decides its lifetime.
            // We do NOT own the inner stream either; we only wrap it.
            base.Dispose(disposing);
        }
    }
}
