namespace Niles.IO
{
    public interface IArtifact
    {
        public string Identity { get; }
        public string? Hash { get; }

        public ArtifactType Type { get; }
    }

    public enum ArtifactType
    {
        Unknown = 0,
        Url,
        File,
        /// <summary>
        /// File not present on filesystem as individual file, e.g. file inside archive
        /// </summary>
        VirtualFile
    }

    public enum ArtifactStatus
    {
        Pending,
        InProgress,
        Completed,
        Skipped,
        Failed
    }

    public static class ArtifactStatusExtension
    {
        public static bool IsProcessed(this ArtifactStatus status)
        {
            if (status == ArtifactStatus.Completed || status == ArtifactStatus.Skipped) return true;
            return false;
        }
    }
}