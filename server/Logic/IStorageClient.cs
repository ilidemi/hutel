namespace hutel.Logic
{
    public interface IStorageClient
    {
        string ReadAll(string path);

        void WriteAll(string path, string contents);

        bool Exists(string path);

        void Copy(string source, string dest);

        void Delete(string path);
    }
}