namespace Bureau.Core.FileSignature
{
    public sealed class GifFileSignature : IFileSignature
    {
        // "GIF87a" or "GIF89a"
        public bool IsMatch(ReadOnlySpan<byte> b) =>
            b.Length >= 6 && ((b[0], b[1], b[2], b[3], b[4], b[5]) == ((byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'7', (byte)'a')
                           || (b[0], b[1], b[2], b[3], b[4], b[5]) == ((byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a'));

        public IEnumerable<string> Extensions => new[] { "gif" };
    }
}
