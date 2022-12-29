using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Muffin.Common.Util
{
    public static class ZipArchiveHelper
    {
        public static async Task<ZipArchiveEntry> CreateEntryFromStream(this ZipArchive zip, string entryName, Stream stream)
        {
            return await zip.CreateEntryFromStream(entryName, CompressionLevel.NoCompression, stream);
        }

        public static async Task<ZipArchiveEntry> CreateEntryFromStream(this ZipArchive zip, string entryName, CompressionLevel compressionLevel, Stream stream)
        {
            var entry = zip.CreateEntry(entryName, compressionLevel);
            using (var entryStream = entry.Open())
            {
                await stream.CopyToAsync(entryStream);
            }
            return entry;
        }

        public static ZipArchiveEntry CreateEntryFromBytes(this ZipArchive zip, string entryName, byte[] bytes)
        {
            return zip.CreateEntryFromBytes(entryName, CompressionLevel.NoCompression, bytes);
        }

        public static ZipArchiveEntry CreateEntryFromBytes(this ZipArchive zip, string entryName, CompressionLevel compressionLevel, byte[] bytes)
        {
            var entry = zip.CreateEntry(entryName, compressionLevel);
            using (var entryStream = entry.Open())
            {
                entryStream.Write(bytes, 0, bytes.Length);
            }
            return entry;
        }
    }
}
