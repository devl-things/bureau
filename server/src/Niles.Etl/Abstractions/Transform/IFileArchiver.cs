namespace Niles.Etl.Transform
{
    public interface IFileArchiver
    {
        void MoveToArchive(string filePath);
        void MoveToError(string filePath);
    }
}
