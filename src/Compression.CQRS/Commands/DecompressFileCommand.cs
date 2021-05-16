using Compression.Utils.Files;
using MediatR;

namespace Compression.CQRS.Commands
{
    public class DecompressFileCommand : IRequest
    {
        public DecompressFileCommand(int chunkIndex, ConcurrentFileDictionary fileChunks)
        {
            ChunkIndex = chunkIndex;
            FileChunks = fileChunks;
        }

        public int ChunkIndex { get; set; }

        public ConcurrentFileDictionary FileChunks { get; set; }
    }
}
