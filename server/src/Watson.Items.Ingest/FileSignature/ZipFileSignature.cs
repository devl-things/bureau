namespace Bureau.Core.FileSignature
{
    public sealed class ZipFileSignature : IFileSignature
    {
        // PK\x03\x04 (local file header) or PK\x05\x06 (empty archive)
        public bool IsMatch(ReadOnlySpan<byte> b) =>
            b.Length >= 4 && b[0] == 0x50 && b[1] == 0x4B &&
            (b[2] == 0x03 && b[3] == 0x04 || b[2] == 0x05 && b[3] == 0x06);

        public IEnumerable<string> Extensions => ["zip"];
    }
}
