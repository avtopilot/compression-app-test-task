using System;

namespace Compression.CQRS
{
    public interface ICompressionService : IDisposable
    {
        void Compress(string fileNameToCompress, string archiveFileName);

        void Decompress(string archiveFileName, string decompressedFileName);
    }
}
