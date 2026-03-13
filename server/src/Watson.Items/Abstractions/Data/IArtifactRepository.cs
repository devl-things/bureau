using Bureau.Core;
using Niles.IO;

namespace Niles.Data
{
    public interface IArtifactRepository
    {
        public Task<Dictionary<string, ArtifactStatus>> GetStatusByHashesAsync(IEnumerable<string> hashes, CancellationToken cancellationToken);
        Task<ArtifactStatus?> GetStatusByHashAsync(string hash, CancellationToken cancellationToken);

        public Task<ArtifactStatus> GetOrCreateAsync(string jobId, IArtifact artifact, long sizeBytes, CancellationToken cancellationToken);
        public Task<ArtifactStatus> GetOrCreateAsync(string jobId, IArtifact artifact, CancellationToken cancellationToken);
        Task CreatePendingAsync(string jobId, string fileName, string fullPath, string sha256, long sizeBytes, CancellationToken ct);
        /// <summary>
        /// Creates a record associated with a <paramref name="jobId"/> in <seealso cref="ArtifactStatus.InProgress"/> state.
        /// </summary>
        /// <param name="jobId">Identifies the specific job associated with the task being created.</param>
        /// <param name="identity">pseudo identifier based on path and name</param>
        /// <param name="hash">Provides the hash for verifying the file's integrity.</param>
        /// <param name="sizeBytes">Represents the size of the file in bytes.</param>
        /// <param name="cancellationToken">Allows for cancellation of the task if needed.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        Task CreateAsync(string jobId, string identity, string hash, long sizeBytes, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a record associated with a <paramref name="jobId"/> in <seealso cref="ArtifactStatus.Skipped"/> state in case there is no such record with <paramref name="identity"/> yet.
        /// </summary>
        /// <param name="jobId">Identifies the job related to the skipped file.</param>
        /// <param name="identity">pseudo identifier based on path and name</param>
        /// <param name="sizeBytes">Represents the size of the skipped file in bytes.</param>
        /// <param name="cancellationToken">Used to signal cancellation of the asynchronous operation.</param>
        /// <returns>Returns a task that represents the asynchronous operation.</returns>
        Task EnsureSkippedAsync(string jobId, string identity, long sizeBytes, CancellationToken cancellationToken);
        /// <summary>
        /// Marks a record as <seealso cref="ArtifactStatus.InProgress"/> state.
        /// </summary>
        /// <param name="hash">Provides the hash for verifying the file's integrity.</param>
        /// <param name="cancellationToken">Used to signal the cancellation of the operation if needed.</param>
        /// <returns>Returns a task that represents the asynchronous operation.</returns>
        Task MarkInProgressAsync(string hash, CancellationToken cancellationToken);

        /// <summary>
        /// Marks an artifact as <seealso cref="ArtifactStatus.Completed"/> asynchronously.
        /// </summary>
        /// <param name="artifact">Represents the item that is being marked as completed.</param>
        /// <param name="progress">Contains information about the progress of the completion task.</param>
        /// <param name="cancellationToken">Allows the operation to be canceled if needed.</param>
        /// <returns>Returns a task that represents the asynchronous operation.</returns>
        Task MarkCompletedAsync(IArtifact artifact, ProgressInfo progress, CancellationToken cancellationToken);

        Task MarkCompletedAsync(IArtifact artifact, CancellationToken cancellationToken);
        /// <summary>
        /// Marks an artifact as <seealso cref="ArtifactStatus.Skipped"/> asynchronously.
        /// </summary>
        /// <param name="artifact"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task MarkSkippedAsync(IArtifact artifact, CancellationToken cancellationToken);

        /// <summary>
        /// Marks an artifact as <seealso cref="ArtifactStatus.Failed"/> asynchronously.
        /// </summary>
        /// <param name="artifact">Represents the item that is being marked as failed.</param>
        /// <param name="error">Describes the reason for marking the artifact as failed.</param>
        /// <param name="cancellationToken">Allows the operation to be canceled if needed.</param>
        /// <returns>Returns a task that represents the asynchronous operation.</returns>
        Task MarkFailedAsync(IArtifact artifact, string error, CancellationToken cancellationToken);


    }
}
