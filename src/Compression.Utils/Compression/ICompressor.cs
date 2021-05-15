using System;

namespace Compression.Utils.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] data);

        byte[] Decompress(byte[] data);
    }
}
