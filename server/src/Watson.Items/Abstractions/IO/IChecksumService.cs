namespace Niles.IO
{
    public interface IChecksumService
    {
        string ComputeHash(string filePath);
        string ComputeHash(byte[] bytes);
        public Task<string> ComputeHashAsync(Stream data, CancellationToken cancellationToken = default);
    }
}
