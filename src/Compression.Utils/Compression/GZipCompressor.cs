using System.IO;
using System.IO.Compression;

namespace Compression.Utils.Compression
{
    public class GZipCompressor : ICompressor
    {
        public byte[] Compress(byte[] data)
        {
            using (var result = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                {
                    compressionStream.Write(data, 0, data.Length);
                }

                return result.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(output);
                    }

                    return output.ToArray();
                }
            }
        }
    }
}
