using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public interface IFileSystem
    {
        bool CreateDirectory(string path, bool recursive = true);

        Task DeleteDirectoryRecursive(string path);

        void ChangeMode(string path, uint mode);

        long GetFreeSpace(string path);

        bool FileExists(string path);

        bool Rename(string oldPath, string newPath);

        Task<bool> WriteFile(string path, byte[] fileContent);

        Task<bool> WriteFile(string path, string fileContent);

        Task<string> ReadFileContent(string path);
    }
}
