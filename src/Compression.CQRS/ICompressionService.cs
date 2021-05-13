namespace Compression.CQRS
{
    public interface ICompressionService
    {
        void Compress(string fileNameToCompress, string archiveFileName);

        void Decompress(string archiveFileName, string decompressedFileName);
    }
}
