using Compression.Utils.Files;
using MediatR;
using System.IO;

namespace Compression.CQRS.Commands
{
    public class WriteChunkCommand : IRequest<int>
    {
        public WriteChunkCommand(FileInfo outputFile, int chunkIndex, ConcurrentFileDictionary fileChunks)
        {
            OutputFile = outputFile;
            ChunkIndex = chunkIndex;
            FileChunks = fileChunks;
        }

        public FileInfo OutputFile { get; set; }

        public int ChunkIndex { get; set; }

        public ConcurrentFileDictionary FileChunks { get; set; }
    }
}
