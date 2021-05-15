using MediatR;

namespace Compression.CQRS.Commands
{
    public class CompressChunkCommand : IRequest
    {
        public CompressChunkCommand(int chunkIndex)
        {
            ChunkIndex = chunkIndex;
        }

        public int ChunkIndex { get; set; }
    }
}
