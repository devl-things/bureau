using System.IO.Compression;

namespace Niles.IO
{
    public class ZipManager
    {
        private readonly IChecksumService _checksum;

        public ZipManager(IChecksumService checksum)
        {
            _checksum = checksum;
        }
        /// <summary>
        /// Reads a zip entry exactly once, computing SHA-256 (or your algo) and buffering bytes
        /// for optional downstream analysis.
        /// </summary>
        /// <returns>Tuple of (hex hash, MemoryStream with content positioned at end; caller should rewind before use).</returns>
        public async Task<InMemoryFile> ReadZipEntryInMemoryAsync(IArtifact artifact, ZipArchiveEntry entry, CancellationToken ct)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            int capacity = 0;
            if (entry.Length > 0 && entry.Length <= int.MaxValue)
            {
                capacity = (int)entry.Length; // capacity hint to reduce reallocations
            }

            MemoryStream buffer = new MemoryStream(capacity);

            using (Stream entryStream = entry.Open())
            using (TeeReadStream tee = new TeeReadStream(entryStream, buffer))
            {
                // Your existing method will read tee to completion (one pass),
                // while tee mirrors bytes into 'buffer'.
                string hash = await _checksum.ComputeHashAsync(tee, ct);
                return new InMemoryFile(hash, buffer, artifact.Identity, entry.FullName, entry.Name);
            }
        }
    }

    public static class ZipArchiveEntryExtensions
    {
        public static string GetEntryIdentity(this ZipArchiveEntry entry, string zipPath)
        {
            return $"{Path.GetFileName(zipPath)}::{entry.FullName}";
        }
    }
}
