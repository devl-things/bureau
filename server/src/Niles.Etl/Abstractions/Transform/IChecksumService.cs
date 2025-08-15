namespace Niles.Etl.Transform
{
    public interface IChecksumService
    {
        string ComputeSha256(string filePath);
    }
}
