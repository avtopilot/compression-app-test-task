using Compression.Utils.Compression;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Compression.CQRS.Commands.Handlers
{
    class DecompressFileCommandHandler : IRequestHandler<DecompressFileCommand, Unit>
    {
        private readonly ICompressor _compressor;

        public DecompressFileCommandHandler(ICompressor compressor)
        {
            _compressor = compressor;
        }

        public Task<Unit> Handle(DecompressFileCommand request, CancellationToken cancellationToken)
        {
            var chunkToCompress = request.FileChunks.Get(request.ChunkIndex);
            request.FileChunks.AddOrUpdate(request.ChunkIndex, _compressor.Decompress(chunkToCompress));

            return Task.FromResult(Unit.Value);
        }
    }
}
