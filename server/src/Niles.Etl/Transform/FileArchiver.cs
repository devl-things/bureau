namespace Niles.Etl.Transform
{
    public sealed class FileArchiver : IFileArchiver
    {
        private readonly string _archiveFolder;
        private readonly string _errorFolder;

        public FileArchiver(string archiveFolder, string errorFolder)
        {
            _archiveFolder = archiveFolder;
            _errorFolder = errorFolder;
        }

        public void MoveToArchive(string filePath)
        {
            MoveTo(filePath, _archiveFolder);
        }

        public void MoveToError(string filePath)
        {
            MoveTo(filePath, _errorFolder);
        }

        private static void MoveTo(string filePath, string targetFolder)
        {
            string sourceDir = Path.GetDirectoryName(filePath)!;
            string targetDir = Path.IsPathRooted(targetFolder)
                ? targetFolder
                : Path.Combine(sourceDir, targetFolder);

            Directory.CreateDirectory(targetDir);
            string dest = Path.Combine(targetDir, Path.GetFileName(filePath));
            if (File.Exists(dest))
            {
                string unique = Path.Combine(
                    targetDir,
                    Path.GetFileNameWithoutExtension(filePath) + "_" +
                    DateTime.UtcNow.ToString("yyyyMMddHHmmss") +
                    Path.GetExtension(filePath));
                dest = unique;
            }
            File.Move(filePath, dest);
        }
    }
}
