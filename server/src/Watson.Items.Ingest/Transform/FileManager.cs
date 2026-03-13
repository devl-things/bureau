using Bureau.Core;

namespace Niles.Etl.Transform
{
    public sealed class FileManager : IFileManager
    {
        private readonly TimeProvider _timeProvider;
        public FileManager(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public Result MoveTo(string filePath, string targetFolder)
        {
            try
            {
                string sourceDir = Path.GetDirectoryName(filePath)!;
                string targetDir = Path.IsPathRooted(targetFolder)
                    ? targetFolder
                    : Path.Combine(sourceDir, targetFolder);

                Directory.CreateDirectory(targetDir);
                string destination = Path.Combine(targetDir, Path.GetFileName(filePath));
                if (File.Exists(destination))
                {
                    destination = Path.Combine(
                        targetDir, $"{Path.GetFileNameWithoutExtension(filePath)}_{_timeProvider.GetUtcNow():yyyyMMddHHmmss}{Path.GetExtension(filePath)}");
                }
                File.Move(filePath, destination);
            }
            catch (Exception ex)
            {
                return new ResultError($"Failed to move file '{filePath}' to '{targetFolder}'.", ex);
            }
            return true;
        }
    }
}
