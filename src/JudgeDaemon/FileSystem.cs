using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public interface IFileSystem
    {
        bool CreateDirectory(string path, bool recursive = true);

        void ChangeMode(string path, uint mode);

        long GetFreeSpace(string path);

        bool FileExists(string path);

        bool Rename(string oldPath, string newPath);

        Task<bool> WriteFileAsync(string path, byte[] fileContent);
    }
}
