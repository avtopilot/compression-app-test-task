using System;

namespace Compression.Utils.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] data, int dataLength);

        byte[] Decompress(byte[] data);
    }
}
