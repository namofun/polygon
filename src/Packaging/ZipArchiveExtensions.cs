using System.Text;
using System.Threading.Tasks;

namespace System.IO.Compression
{
    public static class CreateZipArchiveEntryExtensions
    {
        const int LINUX644 = -2119958528;

        public static ZipArchiveEntry CreateEntryFromByteArray(this ZipArchive zip, byte[] content, string entry)
        {
            ZipArchiveEntry zipEntry = zip.CreateEntry(entry);
            using (Stream stream = zipEntry.Open()) stream.Write(content, 0, content.Length);
            zipEntry.ExternalAttributes = LINUX644;
            return zipEntry;
        }

        public static ZipArchiveEntry CreateEntryFromString(this ZipArchive zip, string content, string entry)
        {
            ZipArchiveEntry ent = CreateEntryFromByteArray(zip, Encoding.UTF8.GetBytes(content), entry);
            ent.ExternalAttributes = LINUX644;
            return ent;
        }

        public static async Task<ZipArchiveEntry> CreateEntryFromStream(this ZipArchive zip, Stream source, string entry)
        {
            ZipArchiveEntry zipEntry = zip.CreateEntry(entry);
            using (Stream stream = zipEntry.Open()) await source.CopyToAsync(stream);
            zipEntry.ExternalAttributes = LINUX644;
            return zipEntry;
        }

        public static async Task<string> ReadAsStringAsync(this ZipArchiveEntry entry)
        {
            using Stream stream = entry.Open();
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
