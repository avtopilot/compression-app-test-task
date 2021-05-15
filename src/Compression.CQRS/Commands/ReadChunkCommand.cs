using MediatR;
using System.IO;

namespace Compression.CQRS.Commands
{
    public class ReadChunkCommand : IRequest
    {
        public ReadChunkCommand(FileInfo inputFile, int chunkIndex, long startPosition, int bytesCount)
        {
            InputFile = inputFile;
            ChunkIndex = chunkIndex;
            StartPosition = startPosition;
            BytesCount = bytesCount;
        }

        public FileInfo InputFile { get; set; }

        public int ChunkIndex { get; set; }

        public long StartPosition { get; set; }

        public int BytesCount { get; set; }
    }
}
