using Bureau.Core;

namespace Niles.Etl.Transform
{
    public interface IFileManager
    {
        public Result MoveTo(string filePath, string targetFolder);
    }
}
