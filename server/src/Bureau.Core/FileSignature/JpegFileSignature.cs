namespace Bureau.Core.FileSignature
{
    public sealed class JpegFileSignature : IFileSignature
    {
        // FF D8 ... FF D9 (start/end markers; start is enough for quick check)
        public bool IsMatch(ReadOnlySpan<byte> b) =>
            b.Length >= 2 && b[0] == 0xFF && b[1] == 0xD8;

        public IEnumerable<string> Extensions => new[] { "jpg", "jpeg" };
    }
}
