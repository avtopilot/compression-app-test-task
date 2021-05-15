using System.IO;
using System.IO.Compression;

namespace Compression.Utils.Compression
{
    public class GZipCompressor : ICompressor
    {
        public byte[] Compress(byte[] data, int dataLength)
        {
            using (var result = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                {
                    compressionStream.Write(data, 0, dataLength);
                }

                return result.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}
