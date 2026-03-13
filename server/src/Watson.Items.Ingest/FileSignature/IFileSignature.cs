namespace Bureau.Core.FileSignature
{
    public interface IFileSignature
    {
        bool IsMatch(ReadOnlySpan<byte> bytes);
        IEnumerable<string> Extensions { get; }
    }
}
