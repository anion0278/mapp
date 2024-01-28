namespace Mapp.DataAccess
{
    public interface IFileManager
    {
        void WriteAllTextToFile(string fullFilePath, string text);
        string ReadAllTextFromFile(string fullFilePath);
    }
}