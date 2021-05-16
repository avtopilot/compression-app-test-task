using Compression.Utils.Compression;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Compression.CQRS.Commands.Handlers
{
    class CompressChunkCommandHanlder : IRequestHandler<CompressChunkCommand, Unit>
    {
        private readonly ICompressor _compressor;

        public CompressChunkCommandHanlder(ICompressor compressor)
        {
            _compressor = compressor;
        }

        public Task<Unit> Handle(CompressChunkCommand request, CancellationToken cancellationToken)
        {
            var chunkToCompress = request.FileChunks.Get(request.ChunkIndex);
            request.FileChunks.AddOrUpdate(request.ChunkIndex, _compressor.Compress(chunkToCompress));

            return Task.FromResult(Unit.Value);
        }
    }
}
