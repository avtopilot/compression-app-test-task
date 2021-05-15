using MediatR;
using System.IO;

namespace Compression.CQRS.Commands
{
    public class WriteChunkCommand : IRequest<int>
    {
        public WriteChunkCommand(FileInfo outputFile, int chunkIndex)
        {
            OutputFile = outputFile;
            ChunkIndex = chunkIndex;
        }

        public FileInfo OutputFile { get; set; }

        public int ChunkIndex { get; set; }
    }
}
