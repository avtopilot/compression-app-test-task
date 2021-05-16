using Compression.Utils.Files;
using MediatR;
using System.IO;

namespace Compression.CQRS.Commands
{
    public class ReadChunkCommand : IRequest
    {
        public ReadChunkCommand(FileInfo inputFile, int chunkIndex, long startPosition, int bytesCount, ConcurrentFileDictionary fileChunks)
        {
            InputFile = inputFile;
            ChunkIndex = chunkIndex;
            StartPosition = startPosition;
            BytesCount = bytesCount;
            FileChunks = fileChunks;
        }

        public FileInfo InputFile { get; set; }

        public int ChunkIndex { get; set; }

        public long StartPosition { get; set; }

        public int BytesCount { get; set; }

        public ConcurrentFileDictionary FileChunks { get; set; }
    }
}
