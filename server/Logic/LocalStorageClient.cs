using System.IO;

namespace hutel.Logic
{
    public class LocalStorageClient: IStorageClient
    {
        public string ReadAll(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAll(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Copy(string source, string dest)
        {
            File.Copy(source, dest);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }
    }
}