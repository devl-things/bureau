namespace Bureau.Core.FileSignature
{
    public sealed class PngFileSignature : IFileSignature
    {
        // 89 50 4E 47 0D 0A 1A 0A
        public bool IsMatch(ReadOnlySpan<byte> b) =>
            b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 &&
            b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A;

        public IEnumerable<string> Extensions => new[] { "png" };
    }
}
