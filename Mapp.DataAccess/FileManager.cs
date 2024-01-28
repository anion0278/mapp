using System.IO;

namespace Mapp.DataAccess
{
    public class FileManager : IFileManager
    {
        public void WriteAllTextToFile(string fullFilePath, string text)
        {
            File.WriteAllText(fullFilePath, text);
        }

        public string ReadAllTextFromFile(string fullFilePath)
        {
            return File.ReadAllText(fullFilePath);
        }
    }
}