namespace Xylab.Polygon.Judgement.Daemon
{
    public interface IFileSystem
    {
        void CreateDirectory(string path, bool recursive = true);

        void ChangeMode(string path, uint mode);

        long GetFreeSpace(string path);
    }
}
