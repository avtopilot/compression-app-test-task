using Compression.Utils.Files;
using MediatR;

namespace Compression.CQRS.Commands
{
    public class CompressChunkCommand : IRequest
    {
        public CompressChunkCommand(int chunkIndex, ConcurrentFileDictionary fileChunks)
        {
            ChunkIndex = chunkIndex;
            FileChunks = fileChunks;
        }

        public int ChunkIndex { get; set; }

        public ConcurrentFileDictionary FileChunks { get; set; }
    }
}
